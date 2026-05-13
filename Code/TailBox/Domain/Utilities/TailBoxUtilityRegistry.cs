using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sandbox.TailBox;

internal static class TailBoxUtilityRegistry
{
	private static readonly HashSet<string> UnsupportedDisplayValues = new( StringComparer.Ordinal )
	{
		"block", "inline-block", "inline", "inline-flex", "grid", "inline-grid", "contents",
		"flow-root", "table", "inline-table", "table-caption", "table-cell", "table-column",
		"table-column-group", "table-footer-group", "table-header-group", "table-row-group", "table-row",
		"list-item"
	};

	private static readonly HashSet<string> KnownUnsupportedExact = new( StringComparer.Ordinal )
	{
		"container", "sr-only", "not-sr-only", "clearfix", "visible", "invisible", "collapse",
		"resize", "resize-x", "resize-y", "resize-none", "appearance-none", "isolate", "isolation-auto",
		"transform", "transform-gpu", "filter", "backdrop-filter"
	};

	private static readonly string[] KnownUtilityPrefixes =
	{
		"flex-", "basis-", "grow-", "shrink-", "order-", "items-", "self-", "content-", "justify-",
		"place-", "grid-", "grid-cols-", "grid-rows-", "col-", "row-", "auto-cols-", "auto-rows-",
		"static", "relative", "absolute", "fixed", "sticky", "inset-", "inset-x-", "inset-y-",
		"top-", "right-", "bottom-", "left-", "z-", "float-", "clear-",
		"w-", "h-", "size-", "min-w-", "max-w-", "min-h-", "max-h-",
		"p-", "px-", "py-", "pt-", "pr-", "pb-", "pl-", "m-", "mx-", "my-", "mt-", "mr-", "mb-", "ml-",
		"space-x-", "space-y-", "gap-", "gap-x-", "gap-y-",
		"bg-", "from-", "via-", "to-", "text-", "font-", "leading-", "tracking-", "line-clamp-",
		"underline", "overline", "line-through", "no-underline", "decoration-", "underline-offset-",
		"whitespace-", "break-", "truncate", "text-ellipsis", "text-clip", "align-", "list-", "placeholder-",
		"border", "border-", "rounded", "rounded-", "divide-", "outline-", "ring-", "accent-",
		"opacity-", "mix-blend-", "shadow", "shadow-", "blur", "blur-", "brightness-", "contrast-",
		"drop-shadow", "drop-shadow-", "grayscale", "grayscale-", "hue-rotate-", "invert", "invert-",
		"saturate-", "sepia", "sepia-", "backdrop-blur", "backdrop-blur-", "backdrop-brightness-",
		"backdrop-contrast-", "backdrop-grayscale", "backdrop-hue-rotate-", "backdrop-invert",
		"backdrop-opacity-", "backdrop-saturate-", "backdrop-sepia",
		"transition", "transition-", "duration-", "delay-", "ease-", "animate-",
		"origin-", "transform-", "rotate-", "scale-", "scale-x-", "scale-y-", "translate-", "translate-x-",
		"translate-y-", "skew-", "skew-x-", "skew-y-",
		"overflow-", "overflow-x-", "overflow-y-", "overscroll-", "scroll-", "snap-",
		"pointer-events-", "cursor-", "select-", "touch-", "will-change-",
		"aspect-", "object-", "fill-", "stroke-", "stroke-"
	};

	public static bool TryCompile( TailBoxCandidate candidate, TailBoxTheme theme, out List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		declarations = new List<TailBoxDeclaration>();
		skipped = null;

		if ( candidate.IsArbitraryProperty )
			return TryArbitraryProperty( candidate, declarations, out skipped );

		if ( candidate.Modifier is not null && !CanHaveModifier( candidate.Base ) )
		{
			skipped = Skip( candidate, TailBoxSkipReason.UnsupportedModifier, $"Utility '{candidate.Base}' does not support modifier '/{candidate.Modifier}' in TailBox." );
			return false;
		}

		if ( candidate.Negative && !CanBeNegative( candidate.Base ) )
		{
			skipped = Skip( candidate, TailBoxSkipReason.UnsupportedValue, $"Negative form '-{candidate.Base}' is not supported for this utility." );
			return false;
		}

		if ( TryDisplay( candidate, declarations, out skipped ) )
			return true;

		if ( skipped is not null )
			return false;

		if ( TryFlex( candidate, theme, declarations, out skipped ) )
			return true;

		if ( TryPosition( candidate, theme, declarations, out skipped ) )
			return true;

		if ( TrySizing( candidate, theme, declarations, out skipped ) )
			return true;

		if ( TrySpacing( candidate, theme, declarations, out skipped ) )
			return true;

		if ( TryBackground( candidate, theme, declarations, out skipped ) )
			return true;

		if ( TryColorBorderAndRadius( candidate, theme, declarations, out skipped ) )
			return true;

		if ( TryTypography( candidate, theme, declarations, out skipped ) )
			return true;

		if ( TryOverflow( candidate, declarations, out skipped ) )
			return true;

		if ( TryOpacity( candidate, theme, declarations, out skipped ) )
			return true;

		if ( TryPointerCursorAndInteraction( candidate, theme, declarations, out skipped ) )
			return true;

		if ( TryZIndex( candidate, theme, declarations, out skipped ) )
			return true;

		if ( TryTransition( candidate, theme, declarations, out skipped ) )
			return true;

		if ( TryEffects( candidate, theme, declarations, out skipped ) )
			return true;

		if ( TryTransforms( candidate, theme, declarations, out skipped ) )
			return true;

		if ( TryAspectRatio( candidate, theme, declarations, out skipped ) )
			return true;

		if ( IsKnownTailwindUtility( candidate.Base ) )
			skipped = Skip( candidate, TailBoxSkipReason.UnsupportedUtility, $"Tailwind utility '{candidate.Base}' is parsed but is not supported by the s&box-safe registry." );

		return false;
	}

	public static bool IsKnownTailwindUtility( string baseName )
	{
		if ( string.IsNullOrWhiteSpace( baseName ) )
			return false;

		if ( UnsupportedDisplayValues.Contains( baseName ) || KnownUnsupportedExact.Contains( baseName ) )
			return true;

		return KnownUtilityPrefixes.Any( prefix => baseName.StartsWith( prefix, StringComparison.Ordinal ) );
	}

	private static bool TryArbitraryProperty( TailBoxCandidate candidate, List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		skipped = null;
		var property = candidate.ArbitraryProperty?.Trim();
		var value = candidate.ArbitraryValue?.Trim();

		if ( string.IsNullOrWhiteSpace( property ) || string.IsNullOrWhiteSpace( value ) )
		{
			skipped = Skip( candidate, TailBoxSkipReason.InvalidCandidate, "Arbitrary property must include a property and value." );
			return false;
		}

		if ( property.StartsWith( "--", StringComparison.Ordinal ) )
		{
			skipped = Skip( candidate, TailBoxSkipReason.UnsupportedArbitraryProperty, "CSS custom properties are skipped because s&box variable resolution has not been verified." );
			return false;
		}

		var declaration = new TailBoxDeclaration( property, value, candidate.Important );
		if ( !TailBoxStyleCapabilities.AllowsDeclaration( declaration ) )
		{
			skipped = TailBoxStyleCapabilities.IsSupportedProperty( property )
				? Skip( candidate, TailBoxSkipReason.UnsupportedValue, $"s&box does not support value '{value}' for property '{property}'." )
				: Skip( candidate, TailBoxSkipReason.UnsupportedProperty, $"s&box does not support property '{property}'." );
			return false;
		}

		declarations.Add( declaration );
		return true;
	}

	private static bool TryDisplay( TailBoxCandidate candidate, List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		skipped = null;
		return candidate.Base switch
		{
			"flex" => Add( candidate, declarations, "display", "flex" ),
			"hidden" => Add( candidate, declarations, "display", "none" ),
			_ when UnsupportedDisplayValues.Contains( candidate.Base ) => UnsupportedDisplay( candidate, out skipped ),
			_ => false
		};
	}

	private static bool UnsupportedDisplay( TailBoxCandidate candidate, out TailBoxCompileDiagnostic skipped )
	{
		skipped = Skip( candidate, TailBoxSkipReason.UnsupportedValue, $"s&box only verifies display:flex and display:none; Tailwind display utility '{candidate.Base}' is skipped." );
		return false;
	}

	private static bool TryFlex( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		skipped = null;
		if ( AddExact( candidate, declarations, new Dictionary<string, TailBoxDeclaration[]>( StringComparer.Ordinal )
		{
			["flex-row"] = new[] { new TailBoxDeclaration( "flex-direction", "row" ) },
			["flex-row-reverse"] = new[] { new TailBoxDeclaration( "flex-direction", "row-reverse" ) },
			["flex-col"] = new[] { new TailBoxDeclaration( "flex-direction", "column" ) },
			["flex-col-reverse"] = new[] { new TailBoxDeclaration( "flex-direction", "column-reverse" ) },
			["flex-wrap"] = new[] { new TailBoxDeclaration( "flex-wrap", "wrap" ) },
			["flex-nowrap"] = new[] { new TailBoxDeclaration( "flex-wrap", "nowrap" ) },
			["flex-wrap-reverse"] = new[] { new TailBoxDeclaration( "flex-wrap", "wrap-reverse" ) },
			["grow"] = new[] { new TailBoxDeclaration( "flex-grow", "1" ) },
			["grow-0"] = new[] { new TailBoxDeclaration( "flex-grow", "0" ) },
			["shrink"] = new[] { new TailBoxDeclaration( "flex-shrink", "1" ) },
			["shrink-0"] = new[] { new TailBoxDeclaration( "flex-shrink", "0" ) },
			["flex-1"] = new[] { new TailBoxDeclaration( "flex-grow", "1" ), new TailBoxDeclaration( "flex-shrink", "1" ), new TailBoxDeclaration( "flex-basis", "0%" ) },
			["flex-auto"] = new[] { new TailBoxDeclaration( "flex-grow", "1" ), new TailBoxDeclaration( "flex-shrink", "1" ), new TailBoxDeclaration( "flex-basis", "auto" ) },
			["flex-initial"] = new[] { new TailBoxDeclaration( "flex-grow", "0" ), new TailBoxDeclaration( "flex-shrink", "1" ), new TailBoxDeclaration( "flex-basis", "auto" ) },
			["flex-none"] = new[] { new TailBoxDeclaration( "flex-grow", "0" ), new TailBoxDeclaration( "flex-shrink", "0" ), new TailBoxDeclaration( "flex-basis", "auto" ) },
			["items-start"] = new[] { new TailBoxDeclaration( "align-items", "flex-start" ) },
			["items-end"] = new[] { new TailBoxDeclaration( "align-items", "flex-end" ) },
			["items-center"] = new[] { new TailBoxDeclaration( "align-items", "center" ) },
			["items-baseline"] = new[] { new TailBoxDeclaration( "align-items", "baseline" ) },
			["items-stretch"] = new[] { new TailBoxDeclaration( "align-items", "stretch" ) },
			["self-auto"] = new[] { new TailBoxDeclaration( "align-self", "auto" ) },
			["self-start"] = new[] { new TailBoxDeclaration( "align-self", "flex-start" ) },
			["self-end"] = new[] { new TailBoxDeclaration( "align-self", "flex-end" ) },
			["self-center"] = new[] { new TailBoxDeclaration( "align-self", "center" ) },
			["self-stretch"] = new[] { new TailBoxDeclaration( "align-self", "stretch" ) },
			["self-baseline"] = new[] { new TailBoxDeclaration( "align-self", "baseline" ) },
			["content-start"] = new[] { new TailBoxDeclaration( "align-content", "flex-start" ) },
			["content-end"] = new[] { new TailBoxDeclaration( "align-content", "flex-end" ) },
			["content-center"] = new[] { new TailBoxDeclaration( "align-content", "center" ) },
			["content-between"] = new[] { new TailBoxDeclaration( "align-content", "space-between" ) },
			["content-around"] = new[] { new TailBoxDeclaration( "align-content", "space-around" ) },
			["content-evenly"] = new[] { new TailBoxDeclaration( "align-content", "space-evenly" ) },
			["content-stretch"] = new[] { new TailBoxDeclaration( "align-content", "stretch" ) },
			["justify-start"] = new[] { new TailBoxDeclaration( "justify-content", "flex-start" ) },
			["justify-end"] = new[] { new TailBoxDeclaration( "justify-content", "flex-end" ) },
			["justify-center"] = new[] { new TailBoxDeclaration( "justify-content", "center" ) },
			["justify-between"] = new[] { new TailBoxDeclaration( "justify-content", "space-between" ) },
			["justify-around"] = new[] { new TailBoxDeclaration( "justify-content", "space-around" ) },
			["justify-evenly"] = new[] { new TailBoxDeclaration( "justify-content", "space-evenly" ) },
			["justify-stretch"] = new[] { new TailBoxDeclaration( "justify-content", "stretch" ) }
		} ) )
		{
			return true;
		}

		if ( TryPrefixLength( candidate, "basis-", "flex-basis", theme, declarations ) )
			return true;

		if ( TrySimpleNumberOrArbitrary( candidate, "grow-", "flex-grow", theme, declarations ) )
			return true;

		if ( TrySimpleNumberOrArbitrary( candidate, "shrink-", "flex-shrink", theme, declarations ) )
			return true;

		if ( TryOrder( candidate, theme, declarations ) )
			return true;

		if ( candidate.Base.StartsWith( "place-", StringComparison.Ordinal ) )
		{
			skipped = Skip( candidate, TailBoxSkipReason.UnsupportedUtility, "Tailwind place-* utilities are grid-oriented and are skipped because grid is not verified in s&box." );
			return false;
		}

		return false;
	}

	private static bool TryPosition( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		skipped = null;
		if ( AddExact( candidate, declarations, new Dictionary<string, TailBoxDeclaration[]>( StringComparer.Ordinal )
		{
			["static"] = new[] { new TailBoxDeclaration( "position", "static" ) },
			["relative"] = new[] { new TailBoxDeclaration( "position", "relative" ) },
			["absolute"] = new[] { new TailBoxDeclaration( "position", "absolute" ) }
		} ) )
		{
			return true;
		}

		if ( candidate.Base is "fixed" or "sticky" )
		{
			skipped = Skip( candidate, TailBoxSkipReason.UnsupportedValue, $"s&box position support is verified for static, relative, and absolute; '{candidate.Base}' is skipped." );
			return false;
		}

		if ( TryInsetGroup( candidate, "inset-", new[] { "top", "right", "bottom", "left" }, theme, declarations ) )
			return true;

		if ( TryInsetGroup( candidate, "inset-x-", new[] { "left", "right" }, theme, declarations ) )
			return true;

		if ( TryInsetGroup( candidate, "inset-y-", new[] { "top", "bottom" }, theme, declarations ) )
			return true;

		foreach ( var side in new[] { "top", "right", "bottom", "left" } )
		{
			if ( TryPrefixLength( candidate, side + "-", side, theme, declarations ) )
				return true;
		}

		return false;
	}

	private static bool TryInsetGroup( TailBoxCandidate candidate, string prefix, IEnumerable<string> properties, TailBoxTheme theme, List<TailBoxDeclaration> declarations )
	{
		if ( !candidate.Base.StartsWith( prefix, StringComparison.Ordinal ) )
			return false;

		if ( !TryResolveLength( candidate.Base[prefix.Length..], candidate.Negative, theme, out var value ) )
			return false;

		foreach ( var property in properties )
		{
			declarations.Add( new TailBoxDeclaration( property, value, candidate.Important ) );
		}

		return true;
	}

	private static bool TrySizing( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		skipped = null;
		if ( TryPrefixLength( candidate, "w-", "width", theme, declarations ) )
			return true;

		if ( TryPrefixLength( candidate, "h-", "height", theme, declarations ) )
			return true;

		if ( candidate.Base.StartsWith( "size-", StringComparison.Ordinal ) )
		{
			if ( !TryResolveLength( candidate.Base["size-".Length..], candidate.Negative, theme, out var value ) )
				return false;

			declarations.Add( new TailBoxDeclaration( "width", value, candidate.Important ) );
			declarations.Add( new TailBoxDeclaration( "height", value, candidate.Important ) );
			return true;
		}

		return TryPrefixLength( candidate, "min-w-", "min-width", theme, declarations )
			|| TryPrefixLength( candidate, "max-w-", "max-width", theme, declarations )
			|| TryPrefixLength( candidate, "min-h-", "min-height", theme, declarations )
			|| TryPrefixLength( candidate, "max-h-", "max-height", theme, declarations );
	}

	private static bool TrySpacing( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		skipped = null;
		var specs = new (string Prefix, string[] Properties, bool AllowsNegative)[]
		{
			("px-", new[] { "padding-left", "padding-right" }, false),
			("py-", new[] { "padding-top", "padding-bottom" }, false),
			("pt-", new[] { "padding-top" }, false),
			("pr-", new[] { "padding-right" }, false),
			("pb-", new[] { "padding-bottom" }, false),
			("pl-", new[] { "padding-left" }, false),
			("p-", new[] { "padding" }, false),
			("mx-", new[] { "margin-left", "margin-right" }, true),
			("my-", new[] { "margin-top", "margin-bottom" }, true),
			("mt-", new[] { "margin-top" }, true),
			("mr-", new[] { "margin-right" }, true),
			("mb-", new[] { "margin-bottom" }, true),
			("ml-", new[] { "margin-left" }, true),
			("m-", new[] { "margin" }, true),
			("gap-x-", new[] { "column-gap" }, false),
			("gap-y-", new[] { "row-gap" }, false),
			("gap-", new[] { "gap" }, false)
		};

		foreach ( var spec in specs )
		{
			if ( !candidate.Base.StartsWith( spec.Prefix, StringComparison.Ordinal ) )
				continue;

			if ( candidate.Negative && !spec.AllowsNegative )
			{
				skipped = Skip( candidate, TailBoxSkipReason.UnsupportedValue, $"Negative values are not supported for '{spec.Prefix[..^1]}' utilities." );
				return false;
			}

			if ( !TryResolveLength( candidate.Base[spec.Prefix.Length..], candidate.Negative, theme, out var value ) )
				return false;

			foreach ( var property in spec.Properties )
			{
				declarations.Add( new TailBoxDeclaration( property, value, candidate.Important ) );
			}

			return true;
		}

		if ( candidate.Base.StartsWith( "space-x-", StringComparison.Ordinal ) || candidate.Base.StartsWith( "space-y-", StringComparison.Ordinal ) )
		{
			skipped = Skip( candidate, TailBoxSkipReason.UnsupportedSelectorVariant, "Tailwind space-* utilities require child selectors; TailBox skips them until nested selector support is verified in s&box." );
			return false;
		}

		return false;
	}

	private static bool TryBackground( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		skipped = null;
		if ( AddExact( candidate, declarations, new Dictionary<string, TailBoxDeclaration[]>( StringComparer.Ordinal )
		{
			["bg-cover"] = new[] { new TailBoxDeclaration( "background-size", "cover" ) },
			["bg-contain"] = new[] { new TailBoxDeclaration( "background-size", "contain" ) },
			["bg-auto"] = new[] { new TailBoxDeclaration( "background-size", "auto" ) },
			["bg-center"] = new[] { new TailBoxDeclaration( "background-position", "center" ) },
			["bg-top"] = new[] { new TailBoxDeclaration( "background-position", "top" ) },
			["bg-right"] = new[] { new TailBoxDeclaration( "background-position", "right" ) },
			["bg-bottom"] = new[] { new TailBoxDeclaration( "background-position", "bottom" ) },
			["bg-left"] = new[] { new TailBoxDeclaration( "background-position", "left" ) },
			["bg-no-repeat"] = new[] { new TailBoxDeclaration( "background-repeat", "no-repeat" ) },
			["bg-repeat"] = new[] { new TailBoxDeclaration( "background-repeat", "repeat" ) },
			["bg-repeat-x"] = new[] { new TailBoxDeclaration( "background-repeat", "repeat-x" ) },
			["bg-repeat-y"] = new[] { new TailBoxDeclaration( "background-repeat", "repeat-y" ) }
		} ) )
		{
			return true;
		}

		if ( candidate.Base.StartsWith( "bg-gradient-", StringComparison.Ordinal )
			|| candidate.Base.StartsWith( "from-", StringComparison.Ordinal )
			|| candidate.Base.StartsWith( "via-", StringComparison.Ordinal )
			|| candidate.Base.StartsWith( "to-", StringComparison.Ordinal ) )
		{
			skipped = Skip( candidate, TailBoxSkipReason.UnsupportedUtility, "Tailwind gradient utilities depend on generated CSS variables and are skipped for pure s&box SCSS output." );
			return false;
		}

		if ( !candidate.Base.StartsWith( "bg-", StringComparison.Ordinal ) )
			return false;

		var key = candidate.Base[3..];
		if ( TryArbitraryWithHint( theme, key, out var hint, out var arbitrary ) )
		{
			if ( hint is "image" or "url" || LooksLikeImageValue( arbitrary ) )
				return Add( candidate, declarations, "background-image", arbitrary );

			if ( hint is "color" || LooksLikeColorValue( arbitrary ) )
			{
				if ( !TryApplyAlphaModifier( candidate, arbitrary, theme, out var color, out skipped ) )
					return false;
				return Add( candidate, declarations, "background-color", color );
			}

			return Add( candidate, declarations, "background", arbitrary );
		}

		if ( TryResolveColor( candidate, key, theme, out var backgroundColor, out skipped ) )
			return Add( candidate, declarations, "background-color", backgroundColor );

		return skipped is not null ? false : false;
	}

	private static bool TryColorBorderAndRadius( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		skipped = null;
		if ( TryBorder( candidate, theme, declarations, out skipped ) )
			return true;

		if ( skipped is not null )
			return false;

		if ( TryRadius( candidate, theme, declarations ) )
			return true;

		if ( candidate.Base.StartsWith( "accent-", StringComparison.Ordinal )
			|| candidate.Base.StartsWith( "fill-", StringComparison.Ordinal )
			|| candidate.Base.StartsWith( "stroke-", StringComparison.Ordinal )
			|| candidate.Base.StartsWith( "outline-", StringComparison.Ordinal )
			|| candidate.Base.StartsWith( "ring-", StringComparison.Ordinal ) )
		{
			skipped = Skip( candidate, TailBoxSkipReason.UnsupportedProperty, $"Utility '{candidate.Base}' maps to properties that are not verified for s&box Razor panels." );
			return false;
		}

		return false;
	}

	private static bool TryBorder( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		skipped = null;
		if ( candidate.Base == "border" )
		{
			var color = theme.Colors.TryGetValue( "border", out var borderColor ) ? borderColor : "rgba( 255, 255, 255, 0.16 )";
			return Add( candidate, declarations, "border", $"1px solid {color}" );
		}

		if ( candidate.Base is "border-x" or "border-y" or "border-t" or "border-r" or "border-b" or "border-l" )
		{
			foreach ( var property in BorderSideProperties( candidate.Base, "width" ) )
			{
				declarations.Add( new TailBoxDeclaration( property, theme.BorderWidths["default"], candidate.Important ) );
			}
			return true;
		}

		if ( !candidate.Base.StartsWith( "border-", StringComparison.Ordinal ) )
			return false;

		var key = candidate.Base[7..];
		var sidePrefix = key.Split( '-' )[0];
		if ( sidePrefix is "x" or "y" or "t" or "r" or "b" or "l" )
		{
			var sideKey = key.Length == sidePrefix.Length ? "default" : key[(sidePrefix.Length + 1)..];
			if ( TryResolveColor( candidate, sideKey, theme, out var sideColor, out skipped ) )
			{
				foreach ( var property in BorderSideProperties( "border-" + sidePrefix, "color" ) )
				{
					declarations.Add( new TailBoxDeclaration( property, sideColor, candidate.Important ) );
				}
				return true;
			}

			if ( skipped is not null )
				return false;

			if ( candidate.Modifier is not null )
			{
				skipped = Skip( candidate, TailBoxSkipReason.UnsupportedModifier, $"Border width utility '{candidate.Base}' does not support opacity modifier '/{candidate.Modifier}'." );
				return false;
			}

			if ( TryResolveBorderWidth( sideKey, theme, out var sideWidth ) )
			{
				foreach ( var property in BorderSideProperties( "border-" + sidePrefix, "width" ) )
				{
					declarations.Add( new TailBoxDeclaration( property, sideWidth, candidate.Important ) );
				}
				return true;
			}

			return false;
		}

		if ( TryResolveColor( candidate, key, theme, out var borderColorValue, out skipped ) )
			return Add( candidate, declarations, "border-color", borderColorValue );

		if ( skipped is not null )
			return false;

		if ( candidate.Modifier is not null )
		{
			skipped = Skip( candidate, TailBoxSkipReason.UnsupportedModifier, $"Border width utility '{candidate.Base}' does not support opacity modifier '/{candidate.Modifier}'." );
			return false;
		}

		if ( TryResolveBorderWidth( key, theme, out var borderWidth ) )
			return Add( candidate, declarations, "border-width", borderWidth );

		if ( key is "solid" or "dashed" or "dotted" or "double" or "hidden" or "none" )
		{
			skipped = Skip( candidate, TailBoxSkipReason.UnsupportedProperty, "border-style is not listed in the verified s&box style property set." );
			return false;
		}

		return false;
	}

	private static IEnumerable<string> BorderSideProperties( string baseName, string suffix )
	{
		return baseName switch
		{
			"border-x" => new[] { "border-left-" + suffix, "border-right-" + suffix },
			"border-y" => new[] { "border-top-" + suffix, "border-bottom-" + suffix },
			"border-t" => new[] { "border-top-" + suffix },
			"border-r" => new[] { "border-right-" + suffix },
			"border-b" => new[] { "border-bottom-" + suffix },
			"border-l" => new[] { "border-left-" + suffix },
			_ => Array.Empty<string>()
		};
	}

	private static bool TryRadius( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations )
	{
		if ( candidate.Base == "rounded" )
			return Add( candidate, declarations, "border-radius", theme.Radii["default"] );

		if ( !candidate.Base.StartsWith( "rounded-", StringComparison.Ordinal ) )
			return false;

		var key = candidate.Base[8..];
		var parts = key.Split( '-', 2 );
		if ( parts.Length == 2 && TryResolveRadius( parts[1], theme, out var sideRadius ) )
		{
			foreach ( var property in RadiusProperties( parts[0] ) )
			{
				declarations.Add( new TailBoxDeclaration( property, sideRadius, candidate.Important ) );
			}

			return declarations.Count > 0;
		}

		if ( TryResolveRadius( key, theme, out var radius ) )
			return Add( candidate, declarations, "border-radius", radius );

		return false;
	}

	private static IEnumerable<string> RadiusProperties( string side )
	{
		return side switch
		{
			"t" => new[] { "border-top-left-radius", "border-top-right-radius" },
			"r" => new[] { "border-top-right-radius", "border-bottom-right-radius" },
			"b" => new[] { "border-bottom-right-radius", "border-bottom-left-radius" },
			"l" => new[] { "border-top-left-radius", "border-bottom-left-radius" },
			"tl" => new[] { "border-top-left-radius" },
			"tr" => new[] { "border-top-right-radius" },
			"br" => new[] { "border-bottom-right-radius" },
			"bl" => new[] { "border-bottom-left-radius" },
			_ => Array.Empty<string>()
		};
	}

	private static bool TryTypography( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		skipped = null;
		if ( candidate.Modifier is null && AddExact( candidate, declarations, new Dictionary<string, TailBoxDeclaration[]>( StringComparer.Ordinal )
		{
			["text-left"] = new[] { new TailBoxDeclaration( "text-align", "left" ) },
			["text-center"] = new[] { new TailBoxDeclaration( "text-align", "center" ) },
			["text-right"] = new[] { new TailBoxDeclaration( "text-align", "right" ) },
			["text-justify"] = new[] { new TailBoxDeclaration( "text-align", "justify" ) },
			["uppercase"] = new[] { new TailBoxDeclaration( "text-transform", "uppercase" ) },
			["lowercase"] = new[] { new TailBoxDeclaration( "text-transform", "lowercase" ) },
			["capitalize"] = new[] { new TailBoxDeclaration( "text-transform", "capitalize" ) },
			["normal-case"] = new[] { new TailBoxDeclaration( "text-transform", "none" ) },
			["italic"] = new[] { new TailBoxDeclaration( "font-style", "italic" ) },
			["not-italic"] = new[] { new TailBoxDeclaration( "font-style", "normal" ) },
			["truncate"] = new[] { new TailBoxDeclaration( "overflow", "hidden" ), new TailBoxDeclaration( "text-overflow", "ellipsis" ), new TailBoxDeclaration( "white-space", "nowrap" ) },
			["text-ellipsis"] = new[] { new TailBoxDeclaration( "text-overflow", "ellipsis" ) },
			["text-clip"] = new[] { new TailBoxDeclaration( "text-overflow", "clip" ) },
			["whitespace-normal"] = new[] { new TailBoxDeclaration( "white-space", "normal" ) },
			["whitespace-nowrap"] = new[] { new TailBoxDeclaration( "white-space", "nowrap" ) },
			["whitespace-pre"] = new[] { new TailBoxDeclaration( "white-space", "pre" ) },
			["whitespace-pre-line"] = new[] { new TailBoxDeclaration( "white-space", "pre-line" ) },
			["whitespace-pre-wrap"] = new[] { new TailBoxDeclaration( "white-space", "pre-wrap" ) },
			["break-normal"] = new[] { new TailBoxDeclaration( "word-break", "normal" ) },
			["break-words"] = new[] { new TailBoxDeclaration( "word-break", "break-word" ) },
			["break-all"] = new[] { new TailBoxDeclaration( "word-break", "break-all" ) },
			["font-thin"] = new[] { new TailBoxDeclaration( "font-weight", "100" ) },
			["font-extralight"] = new[] { new TailBoxDeclaration( "font-weight", "200" ) },
			["font-light"] = new[] { new TailBoxDeclaration( "font-weight", "300" ) },
			["font-normal"] = new[] { new TailBoxDeclaration( "font-weight", "400" ) },
			["font-medium"] = new[] { new TailBoxDeclaration( "font-weight", "500" ) },
			["font-semibold"] = new[] { new TailBoxDeclaration( "font-weight", "600" ) },
			["font-bold"] = new[] { new TailBoxDeclaration( "font-weight", "700" ) },
			["font-extrabold"] = new[] { new TailBoxDeclaration( "font-weight", "800" ) },
			["font-black"] = new[] { new TailBoxDeclaration( "font-weight", "900" ) },
			["underline"] = new[] { new TailBoxDeclaration( "text-decoration-line", "underline" ) },
			["overline"] = new[] { new TailBoxDeclaration( "text-decoration-line", "overline" ) },
			["line-through"] = new[] { new TailBoxDeclaration( "text-decoration-line", "line-through" ) },
			["no-underline"] = new[] { new TailBoxDeclaration( "text-decoration-line", "none" ) },
			["antialiased"] = new[] { new TailBoxDeclaration( "font-smooth", "always" ) },
			["subpixel-antialiased"] = new[] { new TailBoxDeclaration( "font-smooth", "auto" ) }
		} ) )
		{
			return true;
		}

		if ( TryText( candidate, theme, declarations, out skipped ) )
			return true;

		if ( skipped is not null )
			return false;

		if ( TryFont( candidate, theme, declarations ) )
			return true;

		if ( TryLineHeight( candidate, theme, declarations ) )
			return true;

		if ( TryTracking( candidate, theme, declarations ) )
			return true;

		if ( TryDecoration( candidate, theme, declarations, out skipped ) )
			return true;

		if ( TryUnderlineOffset( candidate, theme, declarations ) )
			return true;

		if ( candidate.Base.StartsWith( "line-clamp-", StringComparison.Ordinal ) )
		{
			skipped = Skip( candidate, TailBoxSkipReason.UnsupportedUtility, "line-clamp requires browser overflow behavior that is not part of the verified s&box subset." );
			return false;
		}

		return false;
	}

	private static bool TryText( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		skipped = null;
		if ( !candidate.Base.StartsWith( "text-", StringComparison.Ordinal ) )
			return false;

		var key = candidate.Base[5..];
		if ( TryArbitraryWithHint( theme, key, out var hint, out var arbitrary ) )
		{
			if ( hint is "color" || LooksLikeColorValue( arbitrary ) )
			{
				if ( !TryApplyAlphaModifier( candidate, arbitrary, theme, out var arbitraryColor, out skipped ) )
					return false;
				return Add( candidate, declarations, "color", arbitraryColor );
			}

			declarations.Add( new TailBoxDeclaration( "font-size", arbitrary, candidate.Important ) );
			if ( candidate.Modifier is not null )
			{
				if ( !TryResolveLineHeight( candidate.Modifier, theme, out var lineHeight ) )
				{
					skipped = Skip( candidate, TailBoxSkipReason.UnsupportedModifier, $"Line-height modifier '/{candidate.Modifier}' could not be resolved." );
					declarations.Clear();
					return false;
				}

				declarations.Add( new TailBoxDeclaration( "line-height", lineHeight, candidate.Important ) );
			}
			return true;
		}

		if ( theme.FontSizes.TryGetValue( key, out var fontSize ) )
		{
			declarations.Add( new TailBoxDeclaration( "font-size", fontSize, candidate.Important ) );
			if ( candidate.Modifier is not null )
			{
				if ( !TryResolveLineHeight( candidate.Modifier, theme, out var lineHeight ) )
				{
					skipped = Skip( candidate, TailBoxSkipReason.UnsupportedModifier, $"Line-height modifier '/{candidate.Modifier}' could not be resolved." );
					declarations.Clear();
					return false;
				}

				declarations.Add( new TailBoxDeclaration( "line-height", lineHeight, candidate.Important ) );
			}
			return true;
		}

		if ( TryResolveColor( candidate, key, theme, out var color, out skipped ) )
			return Add( candidate, declarations, "color", color );

		return false;
	}

	private static bool TryFont( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations )
	{
		if ( !candidate.Base.StartsWith( "font-", StringComparison.Ordinal ) )
			return false;

		var key = candidate.Base[5..];
		if ( TryArbitraryWithHint( theme, key, out _, out var arbitrary ) )
		{
			if ( IsNumericValue( arbitrary ) )
				return Add( candidate, declarations, "font-weight", arbitrary );

			return Add( candidate, declarations, "font-family", arbitrary );
		}

		if ( theme.FontFamilies.TryGetValue( key, out var family ) )
			return Add( candidate, declarations, "font-family", family );

		return false;
	}

	private static bool TryLineHeight( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations )
	{
		if ( !candidate.Base.StartsWith( "leading-", StringComparison.Ordinal ) )
			return false;

		if ( !TryResolveLineHeight( candidate.Base[8..], theme, out var value ) )
			return false;

		return Add( candidate, declarations, "line-height", value );
	}

	private static bool TryTracking( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations )
	{
		if ( !candidate.Base.StartsWith( "tracking-", StringComparison.Ordinal ) )
			return false;

		var key = candidate.Base[9..];
		if ( TryArbitraryWithHint( theme, key, out _, out var arbitrary ) )
			return Add( candidate, declarations, "letter-spacing", TailBoxTheme.ApplyNegative( arbitrary, candidate.Negative ) );

		if ( theme.LetterSpacing.TryGetValue( key, out var value ) )
			return Add( candidate, declarations, "letter-spacing", TailBoxTheme.ApplyNegative( value, candidate.Negative ) );

		return false;
	}

	private static bool TryDecoration( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		skipped = null;
		if ( !candidate.Base.StartsWith( "decoration-", StringComparison.Ordinal ) )
			return false;

		var key = candidate.Base[11..];
		if ( TryResolveColor( candidate, key, theme, out var color, out skipped ) )
			return Add( candidate, declarations, "text-decoration-color", color );

		if ( skipped is not null )
			return false;

		if ( theme.TextDecorationThickness.TryGetValue( key, out var thickness ) || TryArbitraryWithHint( theme, key, out _, out thickness ) )
			return Add( candidate, declarations, "text-decoration-thickness", thickness );

		if ( key is "solid" or "double" or "dotted" or "dashed" or "wavy" )
		{
			skipped = Skip( candidate, TailBoxSkipReason.UnsupportedProperty, "text-decoration-style is not listed in the verified s&box style property set." );
			return false;
		}

		return false;
	}

	private static bool TryUnderlineOffset( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations )
	{
		if ( !candidate.Base.StartsWith( "underline-offset-", StringComparison.Ordinal ) )
			return false;

		var key = candidate.Base[17..];
		if ( theme.TextUnderlineOffset.TryGetValue( key, out var value ) || TryArbitraryWithHint( theme, key, out _, out value ) )
			return Add( candidate, declarations, "text-decoration-underline-offset", TailBoxTheme.ApplyNegative( value, candidate.Negative ) );

		return false;
	}

	private static bool TryOverflow( TailBoxCandidate candidate, List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		skipped = null;
		return AddExact( candidate, declarations, new Dictionary<string, TailBoxDeclaration[]>( StringComparer.Ordinal )
		{
			["overflow-visible"] = new[] { new TailBoxDeclaration( "overflow", "visible" ) },
			["overflow-hidden"] = new[] { new TailBoxDeclaration( "overflow", "hidden" ) },
			["overflow-scroll"] = new[] { new TailBoxDeclaration( "overflow", "scroll" ) },
			["overflow-auto"] = new[] { new TailBoxDeclaration( "overflow", "auto" ) },
			["overflow-clip"] = new[] { new TailBoxDeclaration( "overflow", "clip" ) },
			["overflow-x-visible"] = new[] { new TailBoxDeclaration( "overflow-x", "visible" ) },
			["overflow-x-hidden"] = new[] { new TailBoxDeclaration( "overflow-x", "hidden" ) },
			["overflow-x-scroll"] = new[] { new TailBoxDeclaration( "overflow-x", "scroll" ) },
			["overflow-x-auto"] = new[] { new TailBoxDeclaration( "overflow-x", "auto" ) },
			["overflow-x-clip"] = new[] { new TailBoxDeclaration( "overflow-x", "clip" ) },
			["overflow-y-visible"] = new[] { new TailBoxDeclaration( "overflow-y", "visible" ) },
			["overflow-y-hidden"] = new[] { new TailBoxDeclaration( "overflow-y", "hidden" ) },
			["overflow-y-scroll"] = new[] { new TailBoxDeclaration( "overflow-y", "scroll" ) },
			["overflow-y-auto"] = new[] { new TailBoxDeclaration( "overflow-y", "auto" ) },
			["overflow-y-clip"] = new[] { new TailBoxDeclaration( "overflow-y", "clip" ) }
		} );
	}

	private static bool TryOpacity( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		skipped = null;
		if ( !candidate.Base.StartsWith( "opacity-", StringComparison.Ordinal ) )
			return false;

		if ( TryResolveOpacity( candidate.Base[8..], theme, out var value ) )
			return Add( candidate, declarations, "opacity", value );

		return false;
	}

	private static bool TryPointerCursorAndInteraction( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		skipped = null;
		if ( AddExact( candidate, declarations, new Dictionary<string, TailBoxDeclaration[]>( StringComparer.Ordinal )
		{
			["pointer-events-none"] = new[] { new TailBoxDeclaration( "pointer-events", "none" ) },
			["pointer-events-auto"] = new[] { new TailBoxDeclaration( "pointer-events", "auto" ) },
			["pointer-events-all"] = new[] { new TailBoxDeclaration( "pointer-events", "all" ) },
			["cursor-none"] = new[] { new TailBoxDeclaration( "cursor", "none" ) },
			["cursor-pointer"] = new[] { new TailBoxDeclaration( "cursor", "pointer" ) },
			["cursor-progress"] = new[] { new TailBoxDeclaration( "cursor", "progress" ) },
			["cursor-wait"] = new[] { new TailBoxDeclaration( "cursor", "wait" ) },
			["cursor-crosshair"] = new[] { new TailBoxDeclaration( "cursor", "crosshair" ) },
			["cursor-text"] = new[] { new TailBoxDeclaration( "cursor", "text" ) },
			["cursor-move"] = new[] { new TailBoxDeclaration( "cursor", "move" ) },
			["cursor-not-allowed"] = new[] { new TailBoxDeclaration( "cursor", "not-allowed" ) },
			["select-none"] = new[] { new TailBoxDeclaration( "user-select", "none" ) },
			["select-text"] = new[] { new TailBoxDeclaration( "user-select", "text" ) },
			["select-all"] = new[] { new TailBoxDeclaration( "user-select", "all" ) },
			["select-auto"] = new[] { new TailBoxDeclaration( "user-select", "auto" ) }
		} ) )
		{
			return true;
		}

		if ( candidate.Base.StartsWith( "cursor-", StringComparison.Ordinal ) && TryArbitraryWithHint( theme, candidate.Base[7..], out _, out var cursor ) )
			return Add( candidate, declarations, "cursor", cursor );

		if ( candidate.Base.StartsWith( "touch-", StringComparison.Ordinal ) || candidate.Base.StartsWith( "will-change-", StringComparison.Ordinal ) )
		{
			skipped = Skip( candidate, TailBoxSkipReason.UnsupportedProperty, $"Utility '{candidate.Base}' maps to browser interaction properties that are not in the verified s&box subset." );
			return false;
		}

		return false;
	}

	private static bool TryZIndex( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		skipped = null;
		if ( !candidate.Base.StartsWith( "z-", StringComparison.Ordinal ) )
			return false;

		var key = candidate.Base[2..];
		if ( TryArbitraryWithHint( theme, key, out _, out var arbitrary ) )
			return Add( candidate, declarations, "z-index", TailBoxTheme.ApplyNegative( arbitrary, candidate.Negative ) );

		if ( theme.ZIndex.TryGetValue( key, out var token ) )
			return Add( candidate, declarations, "z-index", TailBoxTheme.ApplyNegative( token, candidate.Negative ) );

		if ( int.TryParse( key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var z ) )
			return Add( candidate, declarations, "z-index", candidate.Negative ? (-z).ToString( CultureInfo.InvariantCulture ) : z.ToString( CultureInfo.InvariantCulture ) );

		return false;
	}

	private static bool TryTransition( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		skipped = null;
		if ( AddExact( candidate, declarations, new Dictionary<string, TailBoxDeclaration[]>( StringComparer.Ordinal )
		{
			["transition"] = new[] { new TailBoxDeclaration( "transition", "all 0.15s ease" ) },
			["transition-none"] = new[] { new TailBoxDeclaration( "transition-property", "none" ) },
			["transition-all"] = new[] { new TailBoxDeclaration( "transition-property", "all" ) },
			["transition-colors"] = new[] { new TailBoxDeclaration( "transition-property", "background-color, border-color, color, text-decoration-color" ) },
			["transition-opacity"] = new[] { new TailBoxDeclaration( "transition-property", "opacity" ) },
			["transition-shadow"] = new[] { new TailBoxDeclaration( "transition-property", "box-shadow" ) },
			["transition-transform"] = new[] { new TailBoxDeclaration( "transition-property", "transform" ) }
		} ) )
		{
			return true;
		}

		if ( candidate.Base.StartsWith( "duration-", StringComparison.Ordinal ) )
		{
			if ( TryResolveDuration( candidate.Base[9..], theme, out var duration ) )
				return Add( candidate, declarations, "transition-duration", duration );
			return false;
		}

		if ( candidate.Base.StartsWith( "delay-", StringComparison.Ordinal ) )
		{
			if ( TryResolveDuration( candidate.Base[6..], theme, out var delay ) )
				return Add( candidate, declarations, "transition-delay", delay );
			return false;
		}

		if ( candidate.Base.StartsWith( "ease-", StringComparison.Ordinal ) )
		{
			var key = candidate.Base[5..];
			if ( theme.Easings.TryGetValue( key, out var easing ) || TryArbitraryWithHint( theme, key, out _, out easing ) )
				return Add( candidate, declarations, "transition-timing-function", easing );
			return false;
		}

		return false;
	}

	private static bool TryEffects( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		skipped = null;
		if ( TryShadow( candidate, theme, declarations ) )
			return true;

		if ( TryTextShadow( candidate, theme, declarations ) )
			return true;

		if ( TryFilter( candidate, theme, declarations, out skipped ) )
			return true;

		if ( TryMixBlendMode( candidate, declarations ) )
			return true;

		if ( candidate.Base.StartsWith( "animate-", StringComparison.Ordinal ) )
		{
			var key = candidate.Base[8..];
			if ( theme.Animations.TryGetValue( key, out var animation ) || TryArbitraryWithHint( theme, key, out _, out animation ) )
				return Add( candidate, declarations, "animation", animation );

			skipped = Skip( candidate, TailBoxSkipReason.UnsupportedUtility, "Animation utilities require configured animation tokens or arbitrary animation values; built-in Tailwind keyframes are not generated by TailBox." );
			return false;
		}

		return false;
	}

	private static bool TryShadow( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations )
	{
		if ( candidate.Base == "shadow" )
			return Add( candidate, declarations, "box-shadow", theme.Shadows["default"] );

		if ( !candidate.Base.StartsWith( "shadow-", StringComparison.Ordinal ) )
			return false;

		var key = candidate.Base[7..];
		if ( theme.Shadows.TryGetValue( key, out var shadow ) || TryArbitraryWithHint( theme, key, out _, out shadow ) )
			return Add( candidate, declarations, "box-shadow", shadow );

		return false;
	}

	private static bool TryTextShadow( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations )
	{
		if ( candidate.Base == "text-shadow" )
			return Add( candidate, declarations, "text-shadow", theme.TextShadows["default"] );

		if ( !candidate.Base.StartsWith( "text-shadow-", StringComparison.Ordinal ) )
			return false;

		var key = candidate.Base[12..];
		if ( theme.TextShadows.TryGetValue( key, out var shadow ) || TryArbitraryWithHint( theme, key, out _, out shadow ) )
			return Add( candidate, declarations, "text-shadow", shadow );

		return false;
	}

	private static bool TryFilter( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		skipped = null;
		if ( candidate.Base == "blur" || candidate.Base.StartsWith( "blur-", StringComparison.Ordinal ) )
			return TryBlur( candidate, "blur", "filter-blur", theme, declarations );

		if ( candidate.Base == "backdrop-blur" || candidate.Base.StartsWith( "backdrop-blur-", StringComparison.Ordinal ) )
			return TryBlur( candidate, "backdrop-blur", "backdrop-filter-blur", theme, declarations );

		if ( TryPercentageFilter( candidate, "brightness-", "filter-brightness", declarations )
			|| TryPercentageFilter( candidate, "contrast-", "filter-contrast", declarations )
			|| TryPercentageFilter( candidate, "saturate-", "filter-saturate", declarations )
			|| TryPercentageFilter( candidate, "backdrop-brightness-", "backdrop-filter-brightness", declarations )
			|| TryPercentageFilter( candidate, "backdrop-contrast-", "backdrop-filter-contrast", declarations )
			|| TryPercentageFilter( candidate, "backdrop-saturate-", "backdrop-filter-saturate", declarations ) )
		{
			return true;
		}

		if ( TryAngleFilter( candidate, "hue-rotate-", "filter-hue-rotate", declarations )
			|| TryAngleFilter( candidate, "backdrop-hue-rotate-", "backdrop-filter-hue-rotate", declarations ) )
		{
			return true;
		}

		if ( TryBinaryFilter( candidate, "invert", "invert-", "filter-invert", declarations )
			|| TryBinaryFilter( candidate, "sepia", "sepia-", "filter-sepia", declarations )
			|| TryBinaryFilter( candidate, "grayscale", "grayscale-", "filter-saturate", declarations, "0" )
			|| TryBinaryFilter( candidate, "backdrop-invert", "backdrop-invert-", "backdrop-filter-invert", declarations )
			|| TryBinaryFilter( candidate, "backdrop-sepia", "backdrop-sepia-", "backdrop-filter-sepia", declarations )
			|| TryBinaryFilter( candidate, "backdrop-grayscale", "backdrop-grayscale-", "backdrop-filter-saturate", declarations, "0" ) )
		{
			return true;
		}

		if ( candidate.Base.StartsWith( "drop-shadow", StringComparison.Ordinal ) )
		{
			var key = candidate.Base == "drop-shadow" ? "default" : candidate.Base["drop-shadow-".Length..];
			if ( theme.Shadows.TryGetValue( key, out var shadow ) || TryArbitraryWithHint( theme, key, out _, out shadow ) )
				return Add( candidate, declarations, "filter-drop-shadow", shadow );
		}

		return false;
	}

	private static bool TryBlur( TailBoxCandidate candidate, string utility, string property, TailBoxTheme theme, List<TailBoxDeclaration> declarations )
	{
		var key = candidate.Base == utility ? "default" : candidate.Base[(utility.Length + 1)..];
		var scale = property.StartsWith( "backdrop", StringComparison.Ordinal ) ? theme.BackdropFilters : theme.Filters;
		if ( scale.TryGetValue( key, out var value ) || TryArbitraryWithHint( theme, key, out _, out value ) )
			return Add( candidate, declarations, property, value );

		return false;
	}

	private static bool TryPercentageFilter( TailBoxCandidate candidate, string prefix, string property, List<TailBoxDeclaration> declarations )
	{
		if ( !candidate.Base.StartsWith( prefix, StringComparison.Ordinal ) )
			return false;

		var key = candidate.Base[prefix.Length..];
		if ( TryNumberAsPercentage( key, out var value ) )
			return Add( candidate, declarations, property, value );

		return false;
	}

	private static bool TryAngleFilter( TailBoxCandidate candidate, string prefix, string property, List<TailBoxDeclaration> declarations )
	{
		if ( !candidate.Base.StartsWith( prefix, StringComparison.Ordinal ) )
			return false;

		var key = candidate.Base[prefix.Length..];
		if ( key.StartsWith( "[", StringComparison.Ordinal ) && key.EndsWith( "]", StringComparison.Ordinal ) )
			return Add( candidate, declarations, property, TailBoxText.DecodeArbitraryValue( key[1..^1] ) );

		if ( int.TryParse( key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var degrees ) )
			return Add( candidate, declarations, property, (candidate.Negative ? -degrees : degrees).ToString( CultureInfo.InvariantCulture ) + "deg" );

		return false;
	}

	private static bool TryBinaryFilter( TailBoxCandidate candidate, string exact, string prefix, string property, List<TailBoxDeclaration> declarations, string enabledValue = "1" )
	{
		if ( candidate.Base == exact )
			return Add( candidate, declarations, property, enabledValue );

		if ( !candidate.Base.StartsWith( prefix, StringComparison.Ordinal ) )
			return false;

		var key = candidate.Base[prefix.Length..];
		if ( TryNumberAsPercentage( key, out var value ) )
			return Add( candidate, declarations, property, value );

		return false;
	}

	private static bool TryMixBlendMode( TailBoxCandidate candidate, List<TailBoxDeclaration> declarations )
	{
		const string prefix = "mix-blend-";
		if ( !candidate.Base.StartsWith( prefix, StringComparison.Ordinal ) )
			return false;

		return Add( candidate, declarations, "mix-blend-mode", candidate.Base[prefix.Length..] );
	}

	private static bool TryTransforms( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		skipped = null;
		if ( candidate.Base == "transform-none" )
			return Add( candidate, declarations, "transform", "none" );

		if ( candidate.Base.StartsWith( "transform-[", StringComparison.Ordinal ) && TryArbitraryWithHint( theme, candidate.Base[10..], out _, out var arbitraryTransform ) )
			return Add( candidate, declarations, "transform", arbitraryTransform );

		if ( candidate.Base.StartsWith( "origin-", StringComparison.Ordinal ) )
			return TryTransformOrigin( candidate, declarations );

		if ( candidate.Base.StartsWith( "rotate-", StringComparison.Ordinal )
			|| candidate.Base.StartsWith( "scale-", StringComparison.Ordinal )
			|| candidate.Base.StartsWith( "scale-x-", StringComparison.Ordinal )
			|| candidate.Base.StartsWith( "scale-y-", StringComparison.Ordinal )
			|| candidate.Base.StartsWith( "translate-", StringComparison.Ordinal )
			|| candidate.Base.StartsWith( "translate-x-", StringComparison.Ordinal )
			|| candidate.Base.StartsWith( "translate-y-", StringComparison.Ordinal )
			|| candidate.Base.StartsWith( "skew-", StringComparison.Ordinal )
			|| candidate.Base.StartsWith( "skew-x-", StringComparison.Ordinal )
			|| candidate.Base.StartsWith( "skew-y-", StringComparison.Ordinal ) )
		{
			skipped = Skip( candidate, TailBoxSkipReason.UnsupportedUtility, "Compositional transform utilities require Tailwind CSS variables; use transform-[...] for a single s&box transform value." );
			return false;
		}

		return false;
	}

	private static bool TryTransformOrigin( TailBoxCandidate candidate, List<TailBoxDeclaration> declarations )
	{
		var key = candidate.Base[7..];
		var value = key switch
		{
			"center" => "center",
			"top" => "top",
			"top-right" => "top right",
			"right" => "right",
			"bottom-right" => "bottom right",
			"bottom" => "bottom",
			"bottom-left" => "bottom left",
			"left" => "left",
			"top-left" => "top left",
			_ => null
		};

		if ( value is null )
			return false;

		return Add( candidate, declarations, "transform-origin", value );
	}

	private static bool TryAspectRatio( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations, out TailBoxCompileDiagnostic skipped )
	{
		skipped = null;
		if ( candidate.Base == "aspect-square" )
			return Add( candidate, declarations, "aspect-ratio", "1" );

		if ( candidate.Base == "aspect-video" )
			return Add( candidate, declarations, "aspect-ratio", "16/9" );

		if ( candidate.Base.StartsWith( "aspect-", StringComparison.Ordinal ) && TryArbitraryWithHint( theme, candidate.Base[7..], out _, out var ratio ) )
			return Add( candidate, declarations, "aspect-ratio", ratio );

		return false;
	}

	private static bool TryOrder( TailBoxCandidate candidate, TailBoxTheme theme, List<TailBoxDeclaration> declarations )
	{
		if ( candidate.Base == "order-first" )
			return Add( candidate, declarations, "order", "-9999" );

		if ( candidate.Base == "order-last" )
			return Add( candidate, declarations, "order", "9999" );

		if ( candidate.Base == "order-none" )
			return Add( candidate, declarations, "order", "0" );

		if ( !candidate.Base.StartsWith( "order-", StringComparison.Ordinal ) )
			return false;

		var key = candidate.Base[6..];
		if ( TryArbitraryWithHint( theme, key, out _, out var arbitrary ) )
			return Add( candidate, declarations, "order", TailBoxTheme.ApplyNegative( arbitrary, candidate.Negative ) );

		if ( int.TryParse( key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var order ) )
			return Add( candidate, declarations, "order", candidate.Negative ? (-order).ToString( CultureInfo.InvariantCulture ) : order.ToString( CultureInfo.InvariantCulture ) );

		return false;
	}

	private static bool TryPrefixLength( TailBoxCandidate candidate, string prefix, string property, TailBoxTheme theme, List<TailBoxDeclaration> declarations )
	{
		if ( !candidate.Base.StartsWith( prefix, StringComparison.Ordinal ) )
			return false;

		var key = candidate.Base[prefix.Length..];
		if ( !TryResolveLength( key, candidate.Negative, theme, out var value ) )
			return false;

		if ( key == "screen" && property.Contains( "height", StringComparison.Ordinal ) )
			value = candidate.Negative ? "-100vh" : "100vh";

		return Add( candidate, declarations, property, value );
	}

	private static bool TrySimpleNumberOrArbitrary( TailBoxCandidate candidate, string prefix, string property, TailBoxTheme theme, List<TailBoxDeclaration> declarations )
	{
		if ( !candidate.Base.StartsWith( prefix, StringComparison.Ordinal ) )
			return false;

		var key = candidate.Base[prefix.Length..];
		if ( TryArbitraryWithHint( theme, key, out _, out var arbitrary ) )
			return Add( candidate, declarations, property, arbitrary );

		if ( int.TryParse( key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number ) )
			return Add( candidate, declarations, property, number.ToString( CultureInfo.InvariantCulture ) );

		return false;
	}

	private static bool TryResolveLength( string key, bool negative, TailBoxTheme theme, out string value )
	{
		if ( theme.TryLength( key, theme.Spacing, out value ) )
		{
			value = TailBoxTheme.ApplyNegative( value, negative );
			return true;
		}

		return false;
	}

	private static bool TryResolveBorderWidth( string key, TailBoxTheme theme, out string value )
	{
		value = null;
		if ( theme.BorderWidths.TryGetValue( key, out value ) )
			return true;

		if ( TryArbitraryWithHint( theme, key, out _, out value ) )
			return true;

		if ( int.TryParse( key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var width ) )
		{
			value = width + "px";
			return true;
		}

		return false;
	}

	private static bool TryResolveRadius( string key, TailBoxTheme theme, out string value )
	{
		if ( theme.Radii.TryGetValue( key, out value ) )
			return true;

		if ( TryArbitraryWithHint( theme, key, out _, out value ) )
			return true;

		return false;
	}

	private static bool TryResolveLineHeight( string key, TailBoxTheme theme, out string value )
	{
		if ( theme.LineHeights.TryGetValue( key, out value ) )
			return true;

		if ( TryArbitraryWithHint( theme, key, out _, out value ) )
			return true;

		if ( int.TryParse( key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var lineHeight ) )
		{
			value = lineHeight.ToString( CultureInfo.InvariantCulture );
			return true;
		}

		return false;
	}

	private static bool TryResolveDuration( string key, TailBoxTheme theme, out string value )
	{
		if ( theme.Durations.TryGetValue( key, out value ) )
			return true;

		if ( TryArbitraryWithHint( theme, key, out _, out value ) )
			return true;

		if ( int.TryParse( key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ms ) )
		{
			value = (ms / 1000.0).ToString( "0.###", CultureInfo.InvariantCulture ) + "s";
			return true;
		}

		return false;
	}

	private static bool TryResolveOpacity( string key, TailBoxTheme theme, out string value )
	{
		if ( theme.Opacity.TryGetValue( key, out value ) )
			return true;

		if ( TryArbitraryWithHint( theme, key, out _, out value ) )
			return true;

		if ( double.TryParse( key, NumberStyles.Float, CultureInfo.InvariantCulture, out var opacity ) )
		{
			if ( !key.Contains( '.', StringComparison.Ordinal ) )
				opacity /= 100.0;

			value = opacity.ToString( "0.###", CultureInfo.InvariantCulture );
			return true;
		}

		return false;
	}

	private static bool TryResolveColor( TailBoxCandidate candidate, string key, TailBoxTheme theme, out string color, out TailBoxCompileDiagnostic skipped )
	{
		color = null;
		skipped = null;

		if ( TryArbitraryWithHint( theme, key, out var hint, out var arbitrary ) )
		{
			if ( hint is not null && hint != "color" && !LooksLikeColorValue( arbitrary ) )
				return false;

			if ( !TryApplyAlphaModifier( candidate, arbitrary, theme, out color, out skipped ) )
				return false;

			return true;
		}

		if ( !theme.TryColor( key, out color ) )
			return false;

		return TryApplyAlphaModifier( candidate, color, theme, out color, out skipped );
	}

	private static bool TryApplyAlphaModifier( TailBoxCandidate candidate, string color, TailBoxTheme theme, out string resolved, out TailBoxCompileDiagnostic skipped )
	{
		resolved = color;
		skipped = null;
		if ( candidate.Modifier is null )
			return true;

		if ( !TryResolveOpacity( candidate.Modifier, theme, out var alpha ) )
		{
			skipped = Skip( candidate, TailBoxSkipReason.UnsupportedModifier, $"Opacity modifier '/{candidate.Modifier}' could not be resolved." );
			return false;
		}

		if ( TryApplyAlpha( color, alpha, out resolved ) )
			return true;

		skipped = Skip( candidate, TailBoxSkipReason.UnsupportedModifier, $"Opacity modifier '/{candidate.Modifier}' can only be applied to hex or rgb colors." );
		return false;
	}

	private static bool TryApplyAlpha( string color, string alpha, out string resolved )
	{
		resolved = color;
		var normalized = color.Trim();

		if ( normalized.Equals( "transparent", StringComparison.OrdinalIgnoreCase ) )
		{
			resolved = "transparent";
			return true;
		}

		if ( normalized.StartsWith( "#", StringComparison.Ordinal ) )
		{
			var hex = normalized[1..];
			if ( hex.Length == 3 )
			{
				hex = string.Concat( hex[0], hex[0], hex[1], hex[1], hex[2], hex[2] );
			}

			if ( hex.Length == 6
				&& int.TryParse( hex[0..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r )
				&& int.TryParse( hex[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g )
				&& int.TryParse( hex[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b ) )
			{
				resolved = $"rgba( {r}, {g}, {b}, {alpha} )";
				return true;
			}
		}

		if ( normalized.StartsWith( "rgb(", StringComparison.OrdinalIgnoreCase )
			|| normalized.StartsWith( "rgba(", StringComparison.OrdinalIgnoreCase ) )
		{
			var open = normalized.IndexOf( "(", StringComparison.Ordinal );
			var close = normalized.LastIndexOf( ")", StringComparison.Ordinal );
			if ( open > 0 && close > open )
			{
				var channels = normalized[(open + 1)..close].Split( ',', StringSplitOptions.TrimEntries );
				if ( channels.Length >= 3 )
				{
					resolved = $"rgba( {channels[0]}, {channels[1]}, {channels[2]}, {alpha} )";
					return true;
				}
			}
		}

		return false;
	}

	private static bool TryArbitraryWithHint( TailBoxTheme theme, string key, out string hint, out string value )
	{
		hint = null;
		value = null;
		if ( !theme.TryArbitrary( key, out var arbitrary ) )
			return false;

		var segments = TailBoxText.Segment( arbitrary, ':' );
		if ( segments.Count >= 2 && IsArbitraryTypeHint( segments[0] ) )
		{
			hint = segments[0];
			value = string.Join( ":", segments.Skip( 1 ) );
			return true;
		}

		value = arbitrary;
		return true;
	}

	private static bool IsArbitraryTypeHint( string value )
	{
		return value is "color" or "length" or "size" or "number" or "percentage" or "ratio" or "image" or "url" or "angle" or "family";
	}

	private static bool TryNumberAsPercentage( string key, out string value )
	{
		value = null;
		if ( key.StartsWith( "[", StringComparison.Ordinal ) && key.EndsWith( "]", StringComparison.Ordinal ) )
		{
			value = TailBoxText.DecodeArbitraryValue( key[1..^1] );
			return !string.IsNullOrWhiteSpace( value );
		}

		if ( int.TryParse( key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number ) )
		{
			value = (number / 100.0).ToString( "0.###", CultureInfo.InvariantCulture );
			return true;
		}

		return false;
	}

	private static bool LooksLikeColorValue( string value )
	{
		if ( string.IsNullOrWhiteSpace( value ) )
			return false;

		var lower = value.Trim().ToLowerInvariant();
		return lower.StartsWith( "#", StringComparison.Ordinal )
			|| lower.StartsWith( "rgb(", StringComparison.Ordinal )
			|| lower.StartsWith( "rgba(", StringComparison.Ordinal )
			|| lower.StartsWith( "hsl(", StringComparison.Ordinal )
			|| lower.StartsWith( "hsla(", StringComparison.Ordinal )
			|| lower is "transparent" or "black" or "white" or "currentcolor";
	}

	private static bool LooksLikeImageValue( string value )
	{
		if ( string.IsNullOrWhiteSpace( value ) )
			return false;

		var lower = value.Trim().ToLowerInvariant();
		return lower.StartsWith( "url(", StringComparison.Ordinal )
			|| lower.Contains( "gradient(", StringComparison.Ordinal );
	}

	private static bool IsNumericValue( string value )
	{
		return double.TryParse( value, NumberStyles.Float, CultureInfo.InvariantCulture, out _ );
	}

	private static bool CanHaveModifier( string baseName )
	{
		return baseName.StartsWith( "bg-", StringComparison.Ordinal )
			|| baseName.StartsWith( "text-", StringComparison.Ordinal )
			|| baseName.StartsWith( "border-", StringComparison.Ordinal )
			|| baseName.StartsWith( "decoration-", StringComparison.Ordinal );
	}

	private static bool CanBeNegative( string baseName )
	{
		return baseName.StartsWith( "m-", StringComparison.Ordinal )
			|| baseName.StartsWith( "mx-", StringComparison.Ordinal )
			|| baseName.StartsWith( "my-", StringComparison.Ordinal )
			|| baseName.StartsWith( "mt-", StringComparison.Ordinal )
			|| baseName.StartsWith( "mr-", StringComparison.Ordinal )
			|| baseName.StartsWith( "mb-", StringComparison.Ordinal )
			|| baseName.StartsWith( "ml-", StringComparison.Ordinal )
			|| baseName.StartsWith( "inset-", StringComparison.Ordinal )
			|| baseName.StartsWith( "inset-x-", StringComparison.Ordinal )
			|| baseName.StartsWith( "inset-y-", StringComparison.Ordinal )
			|| baseName.StartsWith( "top-", StringComparison.Ordinal )
			|| baseName.StartsWith( "right-", StringComparison.Ordinal )
			|| baseName.StartsWith( "bottom-", StringComparison.Ordinal )
			|| baseName.StartsWith( "left-", StringComparison.Ordinal )
			|| baseName.StartsWith( "z-", StringComparison.Ordinal )
			|| baseName.StartsWith( "order-", StringComparison.Ordinal )
			|| baseName.StartsWith( "tracking-", StringComparison.Ordinal )
			|| baseName.StartsWith( "underline-offset-", StringComparison.Ordinal );
	}

	private static bool Add( TailBoxCandidate candidate, List<TailBoxDeclaration> declarations, string property, string value )
	{
		declarations.Add( new TailBoxDeclaration( property, value, candidate.Important ) );
		return true;
	}

	private static bool AddExact( TailBoxCandidate candidate, List<TailBoxDeclaration> declarations, Dictionary<string, TailBoxDeclaration[]> rules )
	{
		if ( !rules.TryGetValue( candidate.Base, out var ruleDeclarations ) )
			return false;

		foreach ( var declaration in ruleDeclarations )
		{
			declarations.Add( declaration with { Important = candidate.Important } );
		}

		return true;
	}

	private static TailBoxCompileDiagnostic Skip( TailBoxCandidate candidate, TailBoxSkipReason reason, string detail )
	{
		return new TailBoxCompileDiagnostic
		{
			ClassName = candidate.Original,
			Reason = reason,
			Detail = detail
		};
	}
}
