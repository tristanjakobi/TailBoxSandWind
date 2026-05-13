using System.Collections.Generic;

namespace Sandbox.TailBox;

internal enum TailBoxVariantKind
{
	Pseudo,
	Media,
	Selector,
	Unsupported
}

internal sealed class TailBoxVariant
{
	public string Raw { get; init; } = "";
	public TailBoxVariantKind Kind { get; init; }
	public string SelectorSuffix { get; init; } = "";
	public string Detail { get; init; } = "";
}

internal sealed class TailBoxCandidate
{
	public string Original { get; init; } = "";
	public string Base { get; set; } = "";
	public string Modifier { get; set; }
	public bool Negative { get; init; }
	public bool Important { get; init; }
	public bool IsArbitraryProperty { get; set; }
	public string ArbitraryProperty { get; set; }
	public string ArbitraryValue { get; set; }
	public List<TailBoxVariant> Variants { get; } = new();
}
