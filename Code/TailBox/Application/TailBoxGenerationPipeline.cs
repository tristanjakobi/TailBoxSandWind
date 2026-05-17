using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandbox.TailBox;

internal static class TailBoxGenerationPipeline
{
	public static TailBoxGenerationResult Run( IEnumerable<TailBoxSourceText> sources, TailBoxConfig config, string projectRoot, string outputPath )
	{
		var input = TailBoxPipelineInput.Create( sources, config, projectRoot, outputPath );
		var inventory = CollectCandidates( input );
		var compilation = CompileCandidates( inventory, TailBoxThemeFactory.FromConfig( input.Config ) );
		var scss = TailBoxScssRenderer.Render( compilation.Rules, input.Config, input.Sources.Count );
		return BuildResult( input, inventory, compilation, scss );
	}

	private static TailBoxCandidateInventory CollectCandidates( TailBoxPipelineInput input )
	{
		var occurrences = TailBoxClassExtractor.ExtractClassOccurrencesFromSources( input.Sources );
		var discoveredClasses = new SortedSet<string>( occurrences.Select( occurrence => occurrence.ClassName ), StringComparer.Ordinal );
		var sourceByClass = new Dictionary<string, string>( StringComparer.Ordinal );

		foreach ( var occurrence in occurrences )
		{
			if ( !sourceByClass.ContainsKey( occurrence.ClassName ) )
				sourceByClass[occurrence.ClassName] = occurrence.SourcePath;
		}

		foreach ( var safelisted in input.Config.Safelist ?? Enumerable.Empty<string>() )
		{
			foreach ( var item in SplitClassList( safelisted ) )
			{
				discoveredClasses.Add( item );
			}
		}

		return new TailBoxCandidateInventory( discoveredClasses, sourceByClass );
	}

	private static TailBoxCompilationOutput CompileCandidates( TailBoxCandidateInventory inventory, TailBoxTheme theme )
	{
		var generatedClasses = new List<string>();
		var skippedClasses = new List<string>();
		var skippedItems = new List<TailBoxSkippedClass>();
		var warnings = new List<string>();
		var rules = new List<TailBoxUtilityRule>();

		foreach ( var className in inventory.Classes )
		{
			if ( TailBoxUtilityCompiler.TryCompileDetailed( className, theme, out var rule, out var skipped ) )
			{
				generatedClasses.Add( className );
				rules.Add( rule );
				continue;
			}

			if ( skipped is null )
				continue;

			var withSource = WithSource( skipped, inventory.SourceByClass.TryGetValue( className, out var sourcePath ) ? sourcePath : null );
			skippedItems.Add( withSource );
			skippedClasses.Add( className );

			if ( !string.IsNullOrWhiteSpace( withSource.Detail ) )
				warnings.Add( $"{className}: {withSource.Detail}" );
		}

		AddGapPairRules( rules );
		AddBorderSidePairRules( rules, theme );
		return new TailBoxCompilationOutput( rules, generatedClasses, skippedClasses, skippedItems, warnings );
	}

	private static void AddGapPairRules( List<TailBoxUtilityRule> rules )
	{
		var xRules = rules
			.Where( rule => rule.ClassName.StartsWith( "gap-x-", StringComparison.Ordinal ) )
			.Select( rule => new { Rule = rule, Value = GetDeclarationValue( rule, "column-gap" ) } )
			.Where( item => !string.IsNullOrWhiteSpace( item.Value ) )
			.ToArray();

		var yRules = rules
			.Where( rule => rule.ClassName.StartsWith( "gap-y-", StringComparison.Ordinal ) )
			.Select( rule => new { Rule = rule, Value = GetDeclarationValue( rule, "row-gap" ) } )
			.Where( item => !string.IsNullOrWhiteSpace( item.Value ) )
			.ToArray();

		foreach ( var x in xRules )
		{
			foreach ( var y in yRules )
			{
				var rule = new TailBoxUtilityRule
				{
					ClassName = $"{x.Rule.ClassName}+{y.Rule.ClassName}",
					Selector = $"{x.Rule.Selector}{y.Rule.Selector}"
				};
				rule.Declarations.Add( new TailBoxDeclaration( "gap", $"{y.Value} {x.Value}" ) );
				rule.Declarations.Add( new TailBoxDeclaration( "row-gap", y.Value ) );
				rule.Declarations.Add( new TailBoxDeclaration( "column-gap", x.Value ) );
				rules.Add( rule );
			}
		}
	}

	private static string GetDeclarationValue( TailBoxUtilityRule rule, string property )
	{
		return rule.Declarations.FirstOrDefault( declaration => declaration.Property == property ).Value;
	}

	private static void AddBorderSidePairRules( List<TailBoxUtilityRule> rules, TailBoxTheme theme )
	{
		foreach ( var side in new[] { "t", "r", "b", "l" } )
		{
			var property = side switch
			{
				"t" => "border-top",
				"r" => "border-right",
				"b" => "border-bottom",
				"l" => "border-left",
				_ => ""
			};

			var prefix = "border-" + side;
			var sideRules = rules
				.Where( rule => rule.ClassName == prefix || rule.ClassName.StartsWith( prefix + "-", StringComparison.Ordinal ) )
				.Select( rule => new
				{
					Rule = rule,
					Value = GetDeclarationValue( rule, property ),
					Kind = GetBorderSideRuleKind( rule.ClassName, prefix, theme )
				} )
				.Where( item => !string.IsNullOrWhiteSpace( item.Value ) )
				.ToArray();

			var widthRules = sideRules.Where( item => item.Kind == BorderSideRuleKind.Width ).ToArray();
			var colorRules = sideRules.Where( item => item.Kind == BorderSideRuleKind.Color ).ToArray();

			foreach ( var width in widthRules )
			{
				foreach ( var color in colorRules )
				{
					var borderWidth = GetBorderShorthandWidth( width.Value );
					var borderColor = GetBorderShorthandColor( color.Value );
					if ( string.IsNullOrWhiteSpace( borderWidth ) || string.IsNullOrWhiteSpace( borderColor ) )
						continue;

					var rule = new TailBoxUtilityRule
					{
						ClassName = $"{width.Rule.ClassName}+{color.Rule.ClassName}",
						Selector = $"{width.Rule.Selector}{color.Rule.Selector}"
					};
					rule.Declarations.Add( new TailBoxDeclaration( property, $"{borderWidth} solid {borderColor}" ) );
					rules.Add( rule );
				}
			}
		}
	}

	private enum BorderSideRuleKind
	{
		Unknown,
		Width,
		Color
	}

	private static BorderSideRuleKind GetBorderSideRuleKind( string className, string prefix, TailBoxTheme theme )
	{
		if ( className == prefix )
			return BorderSideRuleKind.Width;

		var key = className[(prefix.Length + 1)..];
		if ( IsArbitraryValue( key ) )
		{
			var arbitrary = TailBoxText.DecodeArbitraryValue( key[1..^1] );
			return LooksLikeColorValue( arbitrary ) ? BorderSideRuleKind.Color : BorderSideRuleKind.Width;
		}

		if ( theme.BorderWidths.ContainsKey( key ) || double.TryParse( key, out _ ) )
			return BorderSideRuleKind.Width;

		return BorderSideRuleKind.Color;
	}

	private static bool IsArbitraryValue( string key )
	{
		return key.StartsWith( "[", StringComparison.Ordinal ) && key.EndsWith( "]", StringComparison.Ordinal );
	}

	private static bool LooksLikeColorValue( string value )
	{
		var lower = value.Trim().ToLowerInvariant();
		return lower.StartsWith( "#", StringComparison.Ordinal )
			|| lower.StartsWith( "rgb(", StringComparison.Ordinal )
			|| lower.StartsWith( "rgba(", StringComparison.Ordinal )
			|| lower.StartsWith( "hsl(", StringComparison.Ordinal )
			|| lower.StartsWith( "hsla(", StringComparison.Ordinal );
	}

	private static string GetBorderShorthandWidth( string value )
	{
		var marker = " solid ";
		var index = value.IndexOf( marker, StringComparison.Ordinal );
		return index < 0 ? "" : value[..index];
	}

	private static string GetBorderShorthandColor( string value )
	{
		var marker = " solid ";
		var index = value.IndexOf( marker, StringComparison.Ordinal );
		return index < 0 ? "" : value[(index + marker.Length)..];
	}

	private static TailBoxGenerationResult BuildResult(
		TailBoxPipelineInput input,
		TailBoxCandidateInventory inventory,
		TailBoxCompilationOutput compilation,
		string scss )
	{
		return new TailBoxGenerationResult
		{
			ProjectRoot = input.ProjectRoot,
			OutputPath = input.OutputPath,
			GeneratedScss = scss,
			ScannedFileCount = input.Sources.Count,
			DiscoveredClassCount = inventory.Classes.Count,
			GeneratedClasses = compilation.GeneratedClasses,
			SkippedClasses = compilation.SkippedClasses,
			Skipped = compilation.Skipped,
			Warnings = compilation.Warnings,
			WroteFile = false
		};
	}

	private static IEnumerable<string> SplitClassList( string value )
	{
		if ( string.IsNullOrWhiteSpace( value ) )
			yield break;

		foreach ( var token in value.Split( new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries ) )
		{
			var cleaned = token.Trim().Trim( ',', ';', '"', '\'' );
			if ( !string.IsNullOrWhiteSpace( cleaned ) )
				yield return cleaned;
		}
	}

	private static TailBoxSkippedClass WithSource( TailBoxCompileDiagnostic skipped, string sourcePath )
	{
		return new TailBoxSkippedClass
		{
			ClassName = skipped.ClassName,
			Reason = skipped.Reason,
			Detail = skipped.Detail,
			SourcePath = sourcePath
		};
	}
}
