using System.Collections.Generic;

namespace Sandbox.TailBox;

internal static class TailBoxThemeFactory
{
	public static TailBoxTheme FromConfig( TailBoxConfig config )
	{
		config ??= TailBoxConfig.CreateDefault();
		return new TailBoxTheme
		{
			Spacing = Copy( config.Spacing ),
			Colors = Copy( config.Colors ),
			FontSizes = Copy( config.FontSizes ),
			Radii = Copy( config.Radii ),
			Shadows = Copy( config.Shadows ),
			Screens = Copy( config.Screens ),
			BorderWidths = Copy( config.BorderWidths ),
			Opacity = Copy( config.Opacity ),
			ZIndex = Copy( config.ZIndex ),
			Durations = Copy( config.Durations ),
			Easings = Copy( config.Easings ),
			FontFamilies = Copy( config.FontFamilies ),
			LineHeights = Copy( config.LineHeights ),
			LetterSpacing = Copy( config.LetterSpacing ),
			TextDecorationThickness = Copy( config.TextDecorationThickness ),
			TextUnderlineOffset = Copy( config.TextUnderlineOffset ),
			TextShadows = Copy( config.TextShadows ),
			Filters = Copy( config.Filters ),
			BackdropFilters = Copy( config.BackdropFilters ),
			Transforms = Copy( config.Transforms ),
			Animations = Copy( config.Animations )
		};
	}

	private static Dictionary<string, string> Copy( Dictionary<string, string> source )
	{
		return source is null
			? new Dictionary<string, string>( System.StringComparer.OrdinalIgnoreCase )
			: new Dictionary<string, string>( source, System.StringComparer.OrdinalIgnoreCase );
	}
}
