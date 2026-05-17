namespace Sandbox.TailBox;

public enum TailBoxSkipReason
{
	InvalidCandidate,
	UnsupportedVariant,
	UnsupportedUtility,
	UnsupportedModifier,
	UnsupportedProperty,
	UnsupportedValue,
	UnsupportedArbitraryProperty,
	UnsupportedSelectorVariant
}

internal sealed class TailBoxCompileDiagnostic
{
	public string ClassName { get; init; } = "";
	public TailBoxSkipReason Reason { get; init; }
	public string Detail { get; init; } = "";
}
