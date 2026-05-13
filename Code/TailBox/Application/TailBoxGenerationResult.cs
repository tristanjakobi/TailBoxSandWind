using System.Collections.Generic;

namespace Sandbox.TailBox;

public sealed class TailBoxGenerationResult
{
	public string ProjectRoot { get; init; } = "";
	public string OutputPath { get; init; } = "";
	public string GeneratedScss { get; init; } = "";
	public int ScannedFileCount { get; init; }
	public int DiscoveredClassCount { get; init; }
	public bool WroteFile { get; init; }
	public IReadOnlyList<string> GeneratedClasses { get; init; } = new List<string>();
	public IReadOnlyList<string> SkippedClasses { get; init; } = new List<string>();
	public IReadOnlyList<TailBoxSkippedClass> Skipped { get; init; } = new List<TailBoxSkippedClass>();
	public IReadOnlyList<string> Warnings { get; init; } = new List<string>();

	public int GeneratedClassCount => GeneratedClasses.Count;
	public int SkippedClassCount => Skipped.Count;
}
