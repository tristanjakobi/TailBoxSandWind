using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandbox.TailBox;

/// <summary>
/// Public entry point for pure, in-memory TailBox generation. Editor project
/// file discovery and output writes live in the editor assembly.
/// </summary>
public sealed class TailBoxGenerator
{
	public TailBoxGenerationResult Generate( string projectRoot )
	{
		throw new NotSupportedException( "TailBox project file generation is editor-only in s&box. Use GenerateFromSources for in-memory generation." );
	}

	public TailBoxGenerationResult Generate( string projectRoot, TailBoxConfig config, bool writeFile = true )
	{
		throw new NotSupportedException( "TailBox project file generation is editor-only in s&box. Use GenerateFromSources for in-memory generation." );
	}

	public TailBoxGenerationResult GenerateFromSources(
		IEnumerable<TailBoxSourceText> sources,
		TailBoxConfig config = null,
		string projectRoot = "",
		string outputPath = "" )
	{
		return TailBoxGenerationPipeline.Run( sources, config, projectRoot, outputPath );
	}

	public IReadOnlyCollection<string> ScanClasses( string projectRoot, TailBoxConfig config )
	{
		throw new NotSupportedException( "TailBox project file scanning is editor-only in s&box. Use ScanClassesFromSources for in-memory scanning." );
	}

	public IReadOnlyCollection<string> ScanClassesFromSources( IEnumerable<TailBoxSourceText> sources )
	{
		return new SortedSet<string>(
			TailBoxClassExtractor.ExtractClassOccurrencesFromSources( sources ).Select( occurrence => occurrence.ClassName ),
			StringComparer.Ordinal );
	}

	public IReadOnlyCollection<string> FindContentFiles( string projectRoot, TailBoxConfig config )
	{
		throw new NotSupportedException( "TailBox project file discovery is editor-only in s&box." );
	}
}
