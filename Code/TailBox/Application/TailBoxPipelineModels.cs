using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandbox.TailBox;

internal sealed record TailBoxPipelineInput(
	IReadOnlyList<TailBoxSourceText> Sources,
	TailBoxConfig Config,
	string ProjectRoot,
	string OutputPath )
{
	public static TailBoxPipelineInput Create( IEnumerable<TailBoxSourceText> sources, TailBoxConfig config, string projectRoot, string outputPath )
	{
		config ??= TailBoxConfig.CreateDefault();
		projectRoot ??= "";

		var sourceList = (sources ?? Enumerable.Empty<TailBoxSourceText>()).ToArray();
		if ( string.IsNullOrWhiteSpace( outputPath ) )
			outputPath = config.GetOutputFullPath( projectRoot );

		return new TailBoxPipelineInput( sourceList, config, projectRoot, outputPath ?? "" );
	}
}

internal sealed record TailBoxCandidateInventory(
	IReadOnlyCollection<string> Classes,
	IReadOnlyDictionary<string, string> SourceByClass );

internal sealed record TailBoxCompilationOutput(
	IReadOnlyList<TailBoxUtilityRule> Rules,
	IReadOnlyList<string> GeneratedClasses,
	IReadOnlyList<string> SkippedClasses,
	IReadOnlyList<TailBoxSkippedClass> Skipped,
	IReadOnlyList<string> Warnings );
