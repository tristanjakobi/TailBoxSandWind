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

		return new TailBoxCompilationOutput( rules, generatedClasses, skippedClasses, skippedItems, warnings );
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
