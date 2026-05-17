using System.Linq;
using System.Text;

namespace Sandbox.TailBox;

internal static class TailBoxUtilityCompiler
{
	public static bool TryCompile( string className, TailBoxTheme theme, out TailBoxUtilityRule rule, out string warning )
	{
		var generated = TryCompileDetailed( className, theme, out rule, out var skipped );
		warning = skipped?.Detail;
		return generated;
	}

	public static bool TryCompileDetailed( string className, TailBoxTheme theme, out TailBoxUtilityRule rule, out TailBoxCompileDiagnostic skipped )
	{
		rule = null;
		skipped = null;
		theme ??= TailBoxTheme.CreateEmpty();

		if ( !TailBoxCandidateParser.TryParse( className, out var candidate, out skipped ) )
			return false;

		foreach ( var variant in candidate.Variants )
		{
			if ( variant.Kind == TailBoxVariantKind.Pseudo )
				continue;

			skipped = new TailBoxCompileDiagnostic
			{
				ClassName = className,
				Reason = variant.Kind switch
				{
					TailBoxVariantKind.Selector => TailBoxSkipReason.UnsupportedSelectorVariant,
					_ => TailBoxSkipReason.UnsupportedVariant
				},
				Detail = variant.Detail
			};
			return false;
		}

		if ( !TailBoxUtilityRegistry.TryCompile( candidate, theme, out var declarations, out skipped ) )
			return false;

		var unsupportedDeclaration = declarations.FirstOrDefault( declaration => !TailBoxStyleCapabilities.AllowsDeclaration( declaration ) );
		if ( unsupportedDeclaration != default )
		{
			skipped = new TailBoxCompileDiagnostic
			{
				ClassName = className,
				Reason = TailBoxStyleCapabilities.IsSupportedProperty( unsupportedDeclaration.Property )
					? TailBoxSkipReason.UnsupportedValue
					: TailBoxSkipReason.UnsupportedProperty,
				Detail = TailBoxStyleCapabilities.IsSupportedProperty( unsupportedDeclaration.Property )
					? $"s&box does not support value '{unsupportedDeclaration.Value}' for property '{unsupportedDeclaration.Property}'."
					: $"s&box does not support property '{unsupportedDeclaration.Property}'."
			};
			return false;
		}

		var pseudoSelector = new StringBuilder();
		foreach ( var variant in candidate.Variants )
		{
			pseudoSelector.Append( variant.SelectorSuffix );
		}

		rule = new TailBoxUtilityRule
		{
			ClassName = className,
			Selector = "." + EscapeClassSelector( className ) + pseudoSelector
		};
		rule.Declarations.AddRange( declarations );
		return true;
	}

	public static string EscapeClassSelector( string className )
	{
		return TailBoxText.EscapeIdentifierForSboxSelector( className );
	}
}
