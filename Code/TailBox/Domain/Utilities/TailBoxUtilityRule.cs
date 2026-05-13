using System.Collections.Generic;

namespace Sandbox.TailBox;

internal sealed class TailBoxUtilityRule
{
	public string ClassName { get; init; } = "";
	public string Selector { get; init; } = "";
	public List<TailBoxDeclaration> Declarations { get; } = new();
}

internal readonly record struct TailBoxDeclaration( string Property, string Value, bool Important = false );
