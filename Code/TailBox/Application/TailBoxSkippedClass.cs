namespace Sandbox.TailBox;

public sealed class TailBoxSkippedClass
{
	public string ClassName { get; init; } = "";
	public TailBoxSkipReason Reason { get; init; }
	public string Detail { get; init; } = "";
	public string SourcePath { get; init; }
}
