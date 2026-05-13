using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sandbox.TailBox;

/// <summary>
/// Configuration for TailBox utility generation in a consuming s&box project.
/// </summary>
public sealed class TailBoxConfig
{
	public const string FileName = "tailbox.config.json";

	public string OutputPath { get; set; } = "Code/tailbox.generated.scss";
	public List<string> Content { get; set; } = CreateDefaultContentGlobs();
	public List<string> Safelist { get; set; } = new();
	public Dictionary<string, string> Spacing { get; set; } = new( StringComparer.OrdinalIgnoreCase );
	public Dictionary<string, string> Colors { get; set; } = new( StringComparer.OrdinalIgnoreCase );
	public Dictionary<string, string> FontSizes { get; set; } = new( StringComparer.OrdinalIgnoreCase );
	public Dictionary<string, string> Radii { get; set; } = new( StringComparer.OrdinalIgnoreCase );
	public Dictionary<string, string> Shadows { get; set; } = new( StringComparer.OrdinalIgnoreCase );
	public Dictionary<string, string> Screens { get; set; } = new( StringComparer.OrdinalIgnoreCase );
	public Dictionary<string, string> BorderWidths { get; set; } = new( StringComparer.OrdinalIgnoreCase );
	public Dictionary<string, string> Opacity { get; set; } = new( StringComparer.OrdinalIgnoreCase );
	public Dictionary<string, string> ZIndex { get; set; } = new( StringComparer.OrdinalIgnoreCase );
	public Dictionary<string, string> Durations { get; set; } = new( StringComparer.OrdinalIgnoreCase );
	public Dictionary<string, string> Easings { get; set; } = new( StringComparer.OrdinalIgnoreCase );
	public Dictionary<string, string> FontFamilies { get; set; } = new( StringComparer.OrdinalIgnoreCase );
	public Dictionary<string, string> LineHeights { get; set; } = new( StringComparer.OrdinalIgnoreCase );
	public Dictionary<string, string> LetterSpacing { get; set; } = new( StringComparer.OrdinalIgnoreCase );
	public Dictionary<string, string> TextDecorationThickness { get; set; } = new( StringComparer.OrdinalIgnoreCase );
	public Dictionary<string, string> TextUnderlineOffset { get; set; } = new( StringComparer.OrdinalIgnoreCase );
	public Dictionary<string, string> TextShadows { get; set; } = new( StringComparer.OrdinalIgnoreCase );
	public Dictionary<string, string> Filters { get; set; } = new( StringComparer.OrdinalIgnoreCase );
	public Dictionary<string, string> BackdropFilters { get; set; } = new( StringComparer.OrdinalIgnoreCase );
	public Dictionary<string, string> Transforms { get; set; } = new( StringComparer.OrdinalIgnoreCase );
	public Dictionary<string, string> Animations { get; set; } = new( StringComparer.OrdinalIgnoreCase );

	[JsonIgnore]
	public string ConfigPath { get; set; }

	public static TailBoxConfig CreateDefault()
	{
		return new TailBoxConfig
		{
			OutputPath = "Code/tailbox.generated.scss",
			Content = CreateDefaultContentGlobs(),
			Safelist = new(),
			Spacing = new( StringComparer.OrdinalIgnoreCase )
			{
				["0"] = "0",
				["px"] = "1px",
				["0.5"] = "2px",
				["1"] = "4px",
				["1.5"] = "6px",
				["2"] = "8px",
				["2.5"] = "10px",
				["3"] = "12px",
				["3.5"] = "14px",
				["4"] = "16px",
				["5"] = "20px",
				["6"] = "24px",
				["7"] = "28px",
				["8"] = "32px",
				["9"] = "36px",
				["10"] = "40px",
				["11"] = "44px",
				["12"] = "48px",
				["14"] = "56px",
				["16"] = "64px",
				["20"] = "80px",
				["24"] = "96px",
				["28"] = "112px",
				["32"] = "128px",
				["36"] = "144px",
				["40"] = "160px",
				["44"] = "176px",
				["48"] = "192px",
				["52"] = "208px",
				["56"] = "224px",
				["60"] = "240px",
				["64"] = "256px",
				["72"] = "288px",
				["80"] = "320px",
				["96"] = "384px"
			},
			Colors = new( StringComparer.OrdinalIgnoreCase )
			{
				["transparent"] = "transparent",
				["black"] = "#000",
				["white"] = "#fff",
				["bg"] = "rgba( 10, 13, 16, 0.92 )",
				["bg-soft"] = "rgba( 24, 29, 34, 0.9 )",
				["panel"] = "rgba( 34, 39, 44, 0.94 )",
				["panel-strong"] = "rgba( 48, 55, 61, 0.98 )",
				["border"] = "rgba( 139, 154, 164, 0.32 )",
				["text"] = "#f1f5f7",
				["muted"] = "#aebbc3",
				["accent"] = "#d7b46a",
				["accent-dark"] = "#7f6531",
				["danger"] = "#c95d5d",
				["good"] = "#78b889"
			},
			FontSizes = new( StringComparer.OrdinalIgnoreCase )
			{
				["xs"] = "12px",
				["sm"] = "13px",
				["base"] = "16px",
				["lg"] = "18px",
				["xl"] = "20px",
				["2xl"] = "24px",
				["3xl"] = "30px",
				["4xl"] = "36px"
			},
			Radii = new( StringComparer.OrdinalIgnoreCase )
			{
				["none"] = "0",
				["sm"] = "4px",
				["default"] = "6px",
				["md"] = "8px",
				["lg"] = "12px",
				["xl"] = "16px",
				["full"] = "9999px"
			},
			Shadows = new( StringComparer.OrdinalIgnoreCase )
			{
				["none"] = "none",
				["sm"] = "0 2px 8px rgba( 0, 0, 0, 0.24 )",
				["default"] = "0 12px 32px rgba( 0, 0, 0, 0.34 )",
				["md"] = "0 14px 36px rgba( 0, 0, 0, 0.38 )",
				["lg"] = "0 18px 48px rgba( 0, 0, 0, 0.44 )"
			},
			Screens = new( StringComparer.OrdinalIgnoreCase )
			{
				["sm"] = "640px",
				["md"] = "768px",
				["lg"] = "1024px",
				["xl"] = "1280px",
				["2xl"] = "1536px"
			},
			BorderWidths = new( StringComparer.OrdinalIgnoreCase )
			{
				["0"] = "0",
				["default"] = "1px",
				["2"] = "2px",
				["4"] = "4px",
				["8"] = "8px"
			},
			Opacity = new( StringComparer.OrdinalIgnoreCase )
			{
				["0"] = "0",
				["5"] = "0.05",
				["10"] = "0.1",
				["20"] = "0.2",
				["25"] = "0.25",
				["30"] = "0.3",
				["40"] = "0.4",
				["50"] = "0.5",
				["60"] = "0.6",
				["70"] = "0.7",
				["75"] = "0.75",
				["80"] = "0.8",
				["90"] = "0.9",
				["95"] = "0.95",
				["100"] = "1"
			},
			ZIndex = new( StringComparer.OrdinalIgnoreCase )
			{
				["0"] = "0",
				["10"] = "10",
				["20"] = "20",
				["30"] = "30",
				["40"] = "40",
				["50"] = "50",
				["auto"] = "auto"
			},
			Durations = new( StringComparer.OrdinalIgnoreCase )
			{
				["0"] = "0s",
				["75"] = "0.075s",
				["100"] = "0.1s",
				["150"] = "0.15s",
				["200"] = "0.2s",
				["300"] = "0.3s",
				["500"] = "0.5s",
				["700"] = "0.7s",
				["1000"] = "1s"
			},
			Easings = new( StringComparer.OrdinalIgnoreCase )
			{
				["linear"] = "linear",
				["in"] = "ease-in",
				["out"] = "ease-out",
				["in-out"] = "ease-in-out"
			},
			FontFamilies = new( StringComparer.OrdinalIgnoreCase )
			{
				["sans"] = "Inter",
				["serif"] = "Georgia",
				["mono"] = "Roboto Mono"
			},
			LineHeights = new( StringComparer.OrdinalIgnoreCase )
			{
				["none"] = "1",
				["tight"] = "1.25",
				["snug"] = "1.375",
				["normal"] = "1.5",
				["relaxed"] = "1.625",
				["loose"] = "2",
				["3"] = "12px",
				["4"] = "16px",
				["5"] = "20px",
				["6"] = "24px",
				["7"] = "28px",
				["8"] = "32px",
				["9"] = "36px",
				["10"] = "40px"
			},
			LetterSpacing = new( StringComparer.OrdinalIgnoreCase )
			{
				["tighter"] = "-0.05em",
				["tight"] = "-0.025em",
				["normal"] = "0",
				["wide"] = "0.025em",
				["wider"] = "0.05em",
				["widest"] = "0.1em"
			},
			TextDecorationThickness = new( StringComparer.OrdinalIgnoreCase )
			{
				["auto"] = "auto",
				["from-font"] = "from-font",
				["0"] = "0",
				["1"] = "1px",
				["2"] = "2px",
				["4"] = "4px",
				["8"] = "8px"
			},
			TextUnderlineOffset = new( StringComparer.OrdinalIgnoreCase )
			{
				["auto"] = "auto",
				["0"] = "0",
				["1"] = "1px",
				["2"] = "2px",
				["4"] = "4px",
				["8"] = "8px"
			},
			TextShadows = new( StringComparer.OrdinalIgnoreCase )
			{
				["none"] = "none",
				["sm"] = "0 1px 2px rgba( 0, 0, 0, 0.4 )",
				["default"] = "0 2px 4px rgba( 0, 0, 0, 0.4 )",
				["lg"] = "0 4px 12px rgba( 0, 0, 0, 0.5 )"
			},
			Filters = new( StringComparer.OrdinalIgnoreCase )
			{
				["none"] = "0",
				["sm"] = "4px",
				["default"] = "8px",
				["md"] = "12px",
				["lg"] = "16px",
				["xl"] = "24px",
				["2xl"] = "40px",
				["3xl"] = "64px"
			},
			BackdropFilters = new( StringComparer.OrdinalIgnoreCase )
			{
				["none"] = "0",
				["sm"] = "4px",
				["default"] = "8px",
				["md"] = "12px",
				["lg"] = "16px",
				["xl"] = "24px",
				["2xl"] = "40px",
				["3xl"] = "64px"
			},
			Transforms = new( StringComparer.OrdinalIgnoreCase )
			{
				["none"] = "none"
			},
			Animations = new( StringComparer.OrdinalIgnoreCase )
			{
				["none"] = "none"
			}
		};
	}

	public static string GetConfigPath( string projectRoot )
	{
		if ( string.IsNullOrWhiteSpace( projectRoot ) )
			throw new ArgumentException( "Project root is required.", nameof( projectRoot ) );

		return CombinePath( projectRoot, FileName );
	}

	public static bool Exists( string projectRoot )
	{
		throw new NotSupportedException( "TailBox config file checks are editor-only in s&box. Use the editor project facade from the infrastructure layer." );
	}

	public static TailBoxConfig Load( string projectRoot )
	{
		throw new NotSupportedException( "TailBox config file loading is editor-only in s&box. Use TailBoxConfig.LoadJson for in-memory JSON." );
	}

	public static TailBoxConfig LoadFile( string path )
	{
		throw new NotSupportedException( "TailBox config file loading is editor-only in s&box. Use TailBoxConfig.LoadJson for in-memory JSON." );
	}

	public static TailBoxConfig LoadJson( string json, string path = null )
	{
		var defaults = CreateDefault();
		defaults.ConfigPath = path;

		try
		{
			var loaded = JsonSerializer.Deserialize<TailBoxConfig>( json, JsonOptions );
			return MergeWithDefaults( defaults, loaded, path );
		}
		catch ( Exception ex )
		{
			throw new InvalidOperationException( $"Unable to load TailBox config JSON{(string.IsNullOrWhiteSpace( path ) ? "" : $": {path}")}", ex );
		}
	}

	public static TailBoxConfig SaveDefault( string projectRoot )
	{
		throw new NotSupportedException( "TailBox config file saving is editor-only in s&box. Use TailBoxConfig.ToJson for in-memory JSON." );
	}

	public void Save( string projectRoot )
	{
		throw new NotSupportedException( "TailBox config file saving is editor-only in s&box. Use TailBoxConfig.ToJson for in-memory JSON." );
	}

	public string ToJson()
	{
		return JsonSerializer.Serialize( this, JsonOptions );
	}

	public string GetOutputFullPath( string projectRoot )
	{
		if ( IsRootedPath( OutputPath ) )
			return NormalizePath( OutputPath );

		return CombinePath( projectRoot, OutputPath );
	}

	private static TailBoxConfig MergeWithDefaults( TailBoxConfig defaults, TailBoxConfig loaded, string path )
	{
		if ( loaded is null )
			return defaults;

		defaults.ConfigPath = path;
		defaults.OutputPath = string.IsNullOrWhiteSpace( loaded.OutputPath ) ? defaults.OutputPath : loaded.OutputPath;
		defaults.Content = loaded.Content is { Count: > 0 } ? new List<string>( loaded.Content ) : defaults.Content;
		defaults.Safelist = loaded.Safelist is null ? defaults.Safelist : new List<string>( loaded.Safelist );
		defaults.Spacing = MergeDictionary( defaults.Spacing, loaded.Spacing );
		defaults.Colors = MergeDictionary( defaults.Colors, loaded.Colors );
		defaults.FontSizes = MergeDictionary( defaults.FontSizes, loaded.FontSizes );
		defaults.Radii = MergeDictionary( defaults.Radii, loaded.Radii );
		defaults.Shadows = MergeDictionary( defaults.Shadows, loaded.Shadows );
		defaults.Screens = MergeDictionary( defaults.Screens, loaded.Screens );
		defaults.BorderWidths = MergeDictionary( defaults.BorderWidths, loaded.BorderWidths );
		defaults.Opacity = MergeDictionary( defaults.Opacity, loaded.Opacity );
		defaults.ZIndex = MergeDictionary( defaults.ZIndex, loaded.ZIndex );
		defaults.Durations = MergeDictionary( defaults.Durations, loaded.Durations );
		defaults.Easings = MergeDictionary( defaults.Easings, loaded.Easings );
		defaults.FontFamilies = MergeDictionary( defaults.FontFamilies, loaded.FontFamilies );
		defaults.LineHeights = MergeDictionary( defaults.LineHeights, loaded.LineHeights );
		defaults.LetterSpacing = MergeDictionary( defaults.LetterSpacing, loaded.LetterSpacing );
		defaults.TextDecorationThickness = MergeDictionary( defaults.TextDecorationThickness, loaded.TextDecorationThickness );
		defaults.TextUnderlineOffset = MergeDictionary( defaults.TextUnderlineOffset, loaded.TextUnderlineOffset );
		defaults.TextShadows = MergeDictionary( defaults.TextShadows, loaded.TextShadows );
		defaults.Filters = MergeDictionary( defaults.Filters, loaded.Filters );
		defaults.BackdropFilters = MergeDictionary( defaults.BackdropFilters, loaded.BackdropFilters );
		defaults.Transforms = MergeDictionary( defaults.Transforms, loaded.Transforms );
		defaults.Animations = MergeDictionary( defaults.Animations, loaded.Animations );
		return defaults;
	}

	private static Dictionary<string, string> MergeDictionary( Dictionary<string, string> defaults, Dictionary<string, string> overrides )
	{
		var result = new Dictionary<string, string>( defaults, StringComparer.OrdinalIgnoreCase );

		if ( overrides is null )
			return result;

		foreach ( var pair in overrides )
		{
			if ( string.IsNullOrWhiteSpace( pair.Key ) || string.IsNullOrWhiteSpace( pair.Value ) )
				continue;

			result[pair.Key] = pair.Value;
		}

		return result;
	}

	private static List<string> CreateDefaultContentGlobs()
	{
		return new()
		{
			"Code/**/*.razor",
			"Libraries/TailBoxSandWind/Code/**/*.razor"
		};
	}

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		WriteIndented = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		PropertyNameCaseInsensitive = true
	};

	private static string CombinePath( string left, string right )
	{
		if ( string.IsNullOrWhiteSpace( left ) )
			return NormalizePath( right );

		if ( string.IsNullOrWhiteSpace( right ) )
			return NormalizePath( left );

		if ( IsRootedPath( right ) )
			return NormalizePath( right );

		return NormalizePath( left.TrimEnd( '/', '\\' ) + "/" + right.TrimStart( '/', '\\' ) );
	}

	private static string NormalizePath( string path )
	{
		return (path ?? "").Replace( '\\', '/' );
	}

	private static bool IsRootedPath( string path )
	{
		if ( string.IsNullOrWhiteSpace( path ) )
			return false;

		return path.StartsWith( "/", StringComparison.Ordinal )
			|| path.StartsWith( "\\", StringComparison.Ordinal )
			|| (path.Length >= 2 && path[1] == ':');
	}
}
