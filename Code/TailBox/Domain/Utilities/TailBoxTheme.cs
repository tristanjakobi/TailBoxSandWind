using System;
using System.Collections.Generic;

namespace Sandbox.TailBox;

internal sealed class TailBoxTheme
{
	public Dictionary<string, string> Spacing { get; init; } = EmptyScale();
	public Dictionary<string, string> Colors { get; init; } = EmptyScale();
	public Dictionary<string, string> FontSizes { get; init; } = EmptyScale();
	public Dictionary<string, string> Radii { get; init; } = EmptyScale();
	public Dictionary<string, string> Shadows { get; init; } = EmptyScale();
	public Dictionary<string, string> Screens { get; init; } = EmptyScale();
	public Dictionary<string, string> BorderWidths { get; init; } = EmptyScale();
	public Dictionary<string, string> Opacity { get; init; } = EmptyScale();
	public Dictionary<string, string> ZIndex { get; init; } = EmptyScale();
	public Dictionary<string, string> Durations { get; init; } = EmptyScale();
	public Dictionary<string, string> Easings { get; init; } = EmptyScale();
	public Dictionary<string, string> FontFamilies { get; init; } = EmptyScale();
	public Dictionary<string, string> LineHeights { get; init; } = EmptyScale();
	public Dictionary<string, string> LetterSpacing { get; init; } = EmptyScale();
	public Dictionary<string, string> TextDecorationThickness { get; init; } = EmptyScale();
	public Dictionary<string, string> TextUnderlineOffset { get; init; } = EmptyScale();
	public Dictionary<string, string> TextShadows { get; init; } = EmptyScale();
	public Dictionary<string, string> Filters { get; init; } = EmptyScale();
	public Dictionary<string, string> BackdropFilters { get; init; } = EmptyScale();
	public Dictionary<string, string> Transforms { get; init; } = EmptyScale();
	public Dictionary<string, string> Animations { get; init; } = EmptyScale();

	public static TailBoxTheme CreateEmpty()
	{
		return new TailBoxTheme();
	}

	public bool TrySpacing( string key, out string value )
	{
		return TryLength( key, Spacing, out value );
	}

	public bool TryLength( string key, Dictionary<string, string> scale, out string value )
	{
		value = null;
		if ( string.IsNullOrWhiteSpace( key ) )
			return false;

		if ( TryArbitrary( key, out value ) )
			return true;

		if ( key == "full" )
			value = "100%";
		else if ( key == "screen" )
			value = "100vw";
		else if ( key == "auto" )
			value = "auto";
		else if ( TryFraction( key, out var fraction ) )
			value = fraction;
		else if ( scale.TryGetValue( key, out var scaled ) )
			value = scaled;

		return value is not null;
	}

	public bool TryColor( string key, out string color )
	{
		if ( TryArbitrary( key, out color ) )
			return true;

		return Colors.TryGetValue( key, out color );
	}

	public bool TryArbitrary( string key, out string value )
	{
		value = null;
		if ( string.IsNullOrWhiteSpace( key ) || key.Length < 3 || key[0] != '[' || key[^1] != ']' )
			return false;

		value = TailBoxText.DecodeArbitraryValue( key[1..^1] );
		return !string.IsNullOrWhiteSpace( value );
	}

	public bool TryFraction( string key, out string value )
	{
		value = null;
		var parts = key.Split( '/', StringSplitOptions.RemoveEmptyEntries );
		if ( parts.Length != 2 )
			return false;

		if ( !double.TryParse( parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var numerator )
			|| !double.TryParse( parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var denominator )
			|| Math.Abs( denominator ) < double.Epsilon )
		{
			return false;
		}

		value = (numerator / denominator * 100.0).ToString( "0.######", System.Globalization.CultureInfo.InvariantCulture ) + "%";
		return true;
	}

	public static string ApplyNegative( string value, bool negative )
	{
		if ( !negative || string.IsNullOrWhiteSpace( value ) || value == "0" || value.StartsWith( "-", StringComparison.Ordinal ) || value == "auto" )
			return value;

		return "-" + value;
	}

	private static Dictionary<string, string> EmptyScale()
	{
		return new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );
	}
}
