using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandbox.TailBox;

internal static class TailBoxCandidateParser
{
	private static readonly Dictionary<string, string> PseudoVariants = new( StringComparer.Ordinal )
	{
		["hover"] = ":hover",
		["active"] = ":active",
		["focus"] = ":focus",
		["intro"] = ":intro",
		["outro"] = ":outro"
	};

	private static readonly HashSet<string> MediaVariants = new( StringComparer.Ordinal )
	{
		"sm", "md", "lg", "xl", "2xl", "max-sm", "max-md", "max-lg", "max-xl", "max-2xl",
		"motion-safe", "motion-reduce", "contrast-more", "contrast-less", "portrait", "landscape",
		"print"
	};

	private static readonly HashSet<string> BrowserSelectorVariants = new( StringComparer.Ordinal )
	{
		"first", "last", "only", "odd", "even", "first-of-type", "last-of-type", "only-of-type",
		"visited", "target", "open", "default", "checked", "indeterminate", "placeholder-shown",
		"autofill", "optional", "required", "valid", "invalid", "user-valid", "user-invalid",
		"in-range", "out-of-range", "read-only", "empty", "focus-within", "focus-visible",
		"enabled", "disabled", "inert", "group", "peer", "has", "not", "aria", "data",
		"nth", "nth-last", "nth-of-type", "nth-last-of-type", "*", "**", "before", "after",
		"placeholder", "selection", "marker", "file", "backdrop", "first-letter", "first-line"
	};

	public static bool TryParse( string className, out TailBoxCandidate candidate, out TailBoxCompileDiagnostic skipped )
	{
		candidate = null;
		skipped = null;

		if ( string.IsNullOrWhiteSpace( className ) )
		{
			skipped = Skip( className, TailBoxSkipReason.InvalidCandidate, "Class name is empty." );
			return false;
		}

		var segments = TailBoxText.Segment( className.Trim(), ':' ).Where( x => x.Length > 0 ).ToList();
		if ( segments.Count == 0 )
		{
			skipped = Skip( className, TailBoxSkipReason.InvalidCandidate, "Class name did not contain a utility segment." );
			return false;
		}

		var baseSegment = segments[^1];
		var important = false;
		if ( baseSegment.StartsWith( "!", StringComparison.Ordinal ) )
		{
			important = true;
			baseSegment = baseSegment[1..];
		}

		if ( baseSegment.EndsWith( "!", StringComparison.Ordinal ) )
		{
			important = true;
			baseSegment = baseSegment[..^1];
		}

		var negative = false;
		if ( baseSegment.StartsWith( "-", StringComparison.Ordinal ) )
		{
			negative = true;
			baseSegment = baseSegment[1..];
		}

		if ( string.IsNullOrWhiteSpace( baseSegment ) )
		{
			skipped = Skip( className, TailBoxSkipReason.InvalidCandidate, "Utility segment is empty after flags were parsed." );
			return false;
		}

		var result = new TailBoxCandidate
		{
			Original = className,
			Negative = negative,
			Important = important
		};

		for ( var i = 0; i < segments.Count - 1; i++ )
		{
			result.Variants.Add( ParseVariant( segments[i] ) );
		}

		if ( baseSegment.StartsWith( "[", StringComparison.Ordinal ) && baseSegment.EndsWith( "]", StringComparison.Ordinal ) )
		{
			var propertyBody = baseSegment[1..^1];
			var propertySegments = TailBoxText.Segment( propertyBody, ':' );
			if ( propertySegments.Count < 2 )
			{
				skipped = Skip( className, TailBoxSkipReason.InvalidCandidate, "Arbitrary property must look like [property:value]." );
				return false;
			}

			result.IsArbitraryProperty = true;
			result.ArbitraryProperty = propertySegments[0];
			result.ArbitraryValue = TailBoxText.DecodeArbitraryValue( string.Join( ":", propertySegments.Skip( 1 ) ) );
			result.Base = baseSegment;
			candidate = result;
			return true;
		}

		var modifierSegments = TailBoxText.Segment( baseSegment, '/' );
		if ( modifierSegments.Count > 2 )
		{
			skipped = Skip( className, TailBoxSkipReason.InvalidCandidate, "Utility contains more than one modifier separator." );
			return false;
		}

		if ( modifierSegments.Count == 2 && !LooksLikeFractionUtility( modifierSegments[0], modifierSegments[1] ) )
		{
			result.Base = modifierSegments[0];
			result.Modifier = DecodeModifier( modifierSegments[1] );
		}
		else
		{
			result.Base = baseSegment;
			result.Modifier = null;
		}

		candidate = result;
		return true;
	}

	private static TailBoxVariant ParseVariant( string raw )
	{
		if ( PseudoVariants.TryGetValue( raw, out var pseudo ) )
		{
			return new TailBoxVariant
			{
				Raw = raw,
				Kind = TailBoxVariantKind.Pseudo,
				SelectorSuffix = pseudo
			};
		}

		if ( MediaVariants.Contains( raw )
			|| raw.StartsWith( "min-", StringComparison.Ordinal )
			|| raw.StartsWith( "max-", StringComparison.Ordinal ) )
		{
			return new TailBoxVariant
			{
				Raw = raw,
				Kind = TailBoxVariantKind.Media,
				Detail = $"Responsive/media variant '{raw}' is parsed but not emitted until s&box media query support is verified."
			};
		}

		if ( raw.StartsWith( "[", StringComparison.Ordinal ) && raw.EndsWith( "]", StringComparison.Ordinal ) )
		{
			return new TailBoxVariant
			{
				Raw = raw,
				Kind = TailBoxVariantKind.Selector,
				Detail = "Arbitrary selector variants are not emitted because nested selector support must be verified in s&box."
			};
		}

		var root = raw;
		var dash = root.IndexOf( '-', StringComparison.Ordinal );
		if ( dash > 0 )
			root = root[..dash];

		if ( BrowserSelectorVariants.Contains( raw ) || BrowserSelectorVariants.Contains( root ) )
		{
			return new TailBoxVariant
			{
				Raw = raw,
				Kind = TailBoxVariantKind.Selector,
				Detail = $"Selector variant '{raw}' is browser-oriented and has not been verified against s&box."
			};
		}

		return new TailBoxVariant
		{
			Raw = raw,
			Kind = TailBoxVariantKind.Unsupported,
			Detail = $"Unsupported TailBox variant '{raw}'."
		};
	}

	private static string DecodeModifier( string modifier )
	{
		if ( modifier.StartsWith( "[", StringComparison.Ordinal ) && modifier.EndsWith( "]", StringComparison.Ordinal ) )
			return TailBoxText.DecodeArbitraryValue( modifier[1..^1] );

		return modifier;
	}

	private static bool LooksLikeFractionUtility( string valueBeforeSlash, string valueAfterSlash )
	{
		if ( string.IsNullOrWhiteSpace( valueBeforeSlash ) || string.IsNullOrWhiteSpace( valueAfterSlash ) )
			return false;

		var lastDash = valueBeforeSlash.LastIndexOf( "-", StringComparison.Ordinal );
		if ( lastDash < 0 || lastDash == valueBeforeSlash.Length - 1 )
			return false;

		var numerator = valueBeforeSlash[(lastDash + 1)..];
		return int.TryParse( numerator, out _ ) && int.TryParse( valueAfterSlash, out _ );
	}

	private static TailBoxCompileDiagnostic Skip( string className, TailBoxSkipReason reason, string detail )
	{
		return new TailBoxCompileDiagnostic
		{
			ClassName = className ?? "",
			Reason = reason,
			Detail = detail
		};
	}
}
