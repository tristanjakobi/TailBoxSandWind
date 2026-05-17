using System.Collections.Generic;

namespace Sandbox.TailBox;

internal static class TailBoxStyleCapabilities
{
	private static readonly HashSet<string> SupportedProperties = new( System.StringComparer.Ordinal )
	{
		"align-content", "align-items", "align-self", "animation", "animation-delay",
		"animation-direction", "animation-duration", "animation-fill-mode", "animation-iteration-count",
		"animation-name", "animation-play-state", "animation-timing-function", "aspect-ratio",
		"backdrop-filter", "backdrop-filter-blur", "backdrop-filter-brightness", "backdrop-filter-contrast",
		"backdrop-filter-hue-rotate", "backdrop-filter-invert", "backdrop-filter-saturate",
		"backdrop-filter-sepia",
		"background", "background-angle", "background-blend-mode",
		"background-color", "background-image", "background-image-tint", "background-position",
		"background-position-x", "background-position-y", "background-repeat", "background-size",
		"background-size-x", "background-size-y", "border", "border-bottom", "border-bottom-color",
		"border-bottom-left-radius", "border-bottom-right-radius", "border-bottom-width", "border-color",
		"border-image", "border-image-tint", "border-image-width-bottom", "border-image-width-left",
		"border-image-width-right", "border-image-width-top", "border-left", "border-left-color",
		"border-left-width", "border-radius", "border-right", "border-right-color", "border-right-width",
		"border-top", "border-top-color", "border-top-left-radius", "border-top-right-radius",
		"border-top-width", "border-width", "bottom", "box-shadow", "color", "column-gap", "content",
		"cursor", "display", "filter", "filter-blur", "filter-border-color", "filter-border-width",
		"filter-brightness", "filter-contrast", "filter-drop-shadow", "filter-hue-rotate",
		"filter-invert", "filter-saturate", "filter-sepia", "filter-tint", "flex-basis",
		"flex-direction", "flex-grow", "flex-shrink", "flex-wrap", "font-color", "font-family",
		"font-size", "font-smooth", "font-style", "font-variant-numeric", "font-weight", "gap",
		"height", "image-rendering", "justify-content", "left", "letter-spacing", "line-height",
		"margin", "margin-bottom", "margin-left", "margin-right", "margin-top", "mask", "mask-angle",
		"mask-image", "mask-mode", "mask-position", "mask-position-x", "mask-position-y", "mask-repeat",
		"mask-scope", "mask-size", "mask-size-x", "mask-size-y", "max-height", "max-width",
		"min-height", "min-width", "mix-blend-mode", "opacity", "order", "overflow", "overflow-x",
		"overflow-y", "padding", "padding-bottom", "padding-left", "padding-right", "padding-top",
		"perspective-origin", "perspective-origin-x", "perspective-origin-y", "pointer-events",
		"position", "right", "row-gap", "text-align", "text-background-angle", "text-decoration",
		"text-decoration-color", "text-decoration-line", "text-decoration-line-through-offset",
		"text-decoration-overline-offset", "text-decoration-skip-ink", "text-decoration-thickness",
		"text-decoration-underline-offset", "text-overflow", "text-shadow", "text-transform", "top",
		"transform", "transform-origin", "transform-origin-x", "transform-origin-y", "transition",
		"transition-delay", "transition-duration", "transition-property", "transition-timing-function",
		"white-space", "width", "word-break", "word-spacing", "z-index", "sound-in", "sound-out",
		"text-stroke", "text-stroke-color", "text-stroke-width"
	};

	public static bool IsSupportedProperty( string property )
	{
		return !string.IsNullOrWhiteSpace( property ) && SupportedProperties.Contains( property );
	}

	public static bool AllowsDeclaration( TailBoxDeclaration declaration )
	{
		if ( !IsSupportedProperty( declaration.Property ) )
			return false;

		if ( !string.IsNullOrWhiteSpace( declaration.Value )
			&& declaration.Value.Contains( "var(", System.StringComparison.OrdinalIgnoreCase ) )
		{
			return false;
		}

		return declaration.Property switch
		{
			"display" => declaration.Value is "flex" or "none",
			"position" => declaration.Value is "static" or "relative" or "absolute",
			"cursor" => true,
			"overflow" or "overflow-x" or "overflow-y" => declaration.Value is "visible" or "hidden" or "scroll",
			"pointer-events" => declaration.Value is "none" or "all" or "auto",
			"white-space" => declaration.Value is "normal" or "nowrap" or "pre" or "pre-line",
			"word-break" => declaration.Value is "normal" or "break-all",
			_ => true
		};
	}
}
