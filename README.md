# TailBox SandWind

TailBox SandWind is a small s&box Razor UI utility generator. It scans Razor files for Tailwind-like class names, parses them with a Tailwind-shaped C# pipeline, and emits ordinary SCSS that s&box can already load through sibling stylesheets, `[StyleSheet]`, or `@import`.

For the code map and generation flow, see [Docs/Architecture.md](Docs/Architecture.md).

## Quick Start

1. Add this library to an s&box project.
2. In the editor, run `Editor > TailBox SandWind > Initialize`.
3. Import the generated stylesheet from a project stylesheet:

```scss
@import "/tailbox.generated.scss";
```

The default config writes `Code/tailbox.generated.scss`, which is addressable from s&box stylesheets as `/tailbox.generated.scss`.

## Configuration

Initialization creates `tailbox.config.json` in the consuming project root. The config is opt-in; if the file is absent, the editor watcher stays idle.

```json
{
  "outputPath": "Code/tailbox.generated.scss",
  "content": [
    "Code/**/*.razor",
    "Libraries/TailBoxSandWind/Code/**/*.razor"
  ],
  "safelist": [],
  "spacing": {},
  "colors": {},
  "fontSizes": {},
  "radii": {},
  "shadows": {},
  "screens": {},
  "borderWidths": {},
  "opacity": {},
  "zIndex": {},
  "durations": {},
  "easings": {},
  "fontFamilies": {},
  "lineHeights": {},
  "letterSpacing": {},
  "textDecorationThickness": {},
  "textUnderlineOffset": {},
  "textShadows": {},
  "filters": {},
  "backdropFilters": {},
  "transforms": {},
  "animations": {}
}
```

Config dictionaries merge with TailBox defaults, so projects only need to specify overrides and extra tokens.

## Demo Component

Add `TailBoxDemo` to any GameObject to bring up a fullscreen showcase menu. At runtime, and in-editor when `RunInEditor` is enabled, it ensures the scene has a `ScreenPanel`, creates a main camera if the scene has no camera, enables mouse UI, and attaches `TailBoxDemoMenu`.

The demo is split into individual screens for layout, spacing, color, borders, typography, position, interaction variants, effects, and arbitrary/important syntax. Existing configs created before the demo should add `Libraries/TailBoxSandWind/Code/**/*.razor` to `content`, then run `Editor > TailBox SandWind > Generate Now`.

## Safelisting

Classes can be safelisted in config or directly in Razor comments:

```razor
@* tailbox safelist: hover:bg-accent intro:opacity-0 bg-[#0d1418] *@
```

## Tailwind Parity Scope

TailBox accepts Tailwind-shaped candidates including variants, important flags, negatives, slash modifiers, arbitrary values, and arbitrary properties. A candidate either emits s&box-safe SCSS or appears in the generation report with a stable unsupported reason.

Supported output is intentionally bounded by s&box style support: flex layout, position/inset, sizing, spacing, colors/backgrounds, borders/radius, typography, overflow, opacity, pointer/cursor, z-index, transitions, transforms where they can be emitted without Tailwind CSS variables, filters, shadows, text decoration, animation tokens, and `hover:`, `active:`, `focus:`, `intro:`, `outro:` pseudo variants.

Responsive variants such as `sm:` and selector variants such as `first:` are parsed and reported, but not emitted until their generated stylesheet forms are verified against the s&box parser.
