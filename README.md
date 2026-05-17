# tailw&

tailw& brings a Tailwind-inspired utility workflow to s&box Razor UI without requiring Tailwind itself at runtime. It scans configured Razor files and safelist entries for utility class names, parses them with a Tailwind-shaped C# pipeline, and writes ordinary s&box-compatible SCSS that can be loaded through sibling stylesheets, `[StyleSheet]`, or `@import`.

It is intentionally a compatibility layer, not a full Tailwind CSS port. Supported classes are emitted as stable generated SCSS, while unsupported classes are kept visible in the generation report with deterministic reasons so teams can see exactly what needs a fallback or future implementation.

The C# API still uses the `Sandbox.TailBox` namespace for compatibility.

For a practical setup walkthrough, see [Docs/UsageGuide.md](Docs/UsageGuide.md). For the code map and generation flow, see [Docs/Architecture.md](Docs/Architecture.md). For the s&box style-property support surface, see [Docs/StyleProperties.md](Docs/StyleProperties.md).

## Quick Start

1. Add this library to an s&box project.
2. In the editor, run `Editor > tailw& > Initialize`.
3. Import the generated stylesheet from a project stylesheet:

```scss
@import "/tailwand.generated.scss";
```

The default config writes `Code/tailwand.generated.scss`, which is addressable from s&box stylesheets as `/tailwand.generated.scss`.

## Configuration

Initialization creates `tailwand.config.json` in the consuming project root. Existing `tailbox.config.json` files are still recognized as legacy configs. The config is opt-in; if neither file is present, the editor watcher stays idle.

```json
{
  "outputPath": "Code/tailwand.generated.scss",
  "content": [
    "Code/**/*.razor",
    "Libraries/tailwand/Code/**/*.razor",
    "Libraries/TailBoxSandWind/Code/**/*.razor"
  ],
  "safelist": [],
  "spacing": {},
  "colors": {},
  "fontSizes": {},
  "radii": {},
  "shadows": {},
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

Config dictionaries merge with tailw& defaults, so projects only need to specify overrides and extra tokens.

## Demo Component

Add `TailBoxDemo` to any GameObject to bring up the tailw& fullscreen showcase menu. At runtime, and in-editor when `RunInEditor` is enabled, it ensures the scene has a `ScreenPanel`, creates a main camera if the scene has no camera, enables mouse UI, and attaches `TailBoxDemoMenu`.

The demo uses a bundled `TailBoxDemoMenu.cs.scss`, so the showcase can render even before a consuming project imports its own generated stylesheet. It is split into individual test screens for flex direction, wrapping, alignment, flex sizing, position, sizing, bounds, padding, margin, gap, text colors, background colors/images, borders, radius, text scale, font/tracking, decoration, overflow, z layering, pointer/cursor, filters, shadows, backdrop filters, opacity/blend, transforms, transitions, animations, masks, intro/outro variants, negatives, and unsupported reporting. Existing configs created before the demo should add `Libraries/TailBoxSandWind/Code/**/*.razor` to `content`, then run `Editor > tailw& > Generate Now`.

## Safelisting

Classes can be safelisted in config or directly in Razor comments:

```razor
@* tailbox safelist: bg-accent z-10 bg-[#0d1418] *@
```

## Tailwind Parity Scope

tailw& accepts Tailwind-shaped candidates including variants, important flags, negatives, slash modifiers, arbitrary values, and arbitrary properties. A candidate either emits s&box-safe SCSS or appears in the generation report with a stable unsupported reason.

Supported output is intentionally bounded by what s&box stylesheets can parse and apply safely.

| Area | Status | Notes |
| --- | --- | --- |
| Content scanning | Supported | Scans configured Razor globs and safelist entries. |
| Config tokens | Supported | Project config can override or extend spacing, colors, font sizes, radii, shadows, borders, opacity tokens, z-index, fonts, line heights, letter spacing, text decoration, filters, backdrop filters, transforms, transitions, and animations. Animation utilities emit declarations only; keyframes stay project-owned. |
| Flex layout | Supported | Includes display, direction, wrapping, grow/shrink, basis, order, alignment, justification, and gap utilities. |
| Position and inset | Supported | Includes `static`, `relative`, `absolute`, inset groups, directional offsets, negative offsets, `auto`, and `full`. |
| Sizing | Supported | Includes width, height, min/max sizing, fractions, `full`, `screen`, arbitrary lengths, and `aspect-*`. |
| Spacing | Supported | Includes margin, padding, directional spacing, negative margins, `auto`, gap, and arbitrary lengths. Negative padding is reported as unsupported. |
| Colors and backgrounds | Supported with limits | Includes theme colors, arbitrary colors, text/background/border opacity modifiers, background images, repeat, position, and size utilities. Tailwind gradient stop utilities such as `from-*` still depend on CSS variables and remain report-only. |
| Borders and radius | Supported with limits | Includes border width, color, side-specific border utilities, and all-corner plus side/corner radius utilities. Side-specific border utilities emit `border-top/right/bottom/left` shorthands because s&box runtime UI does not reliably render split `*-width` plus `*-color` edge declarations. Side/corner radius utilities such as `rounded-t-lg` and `rounded-br-md` emit four-value `border-radius` shorthands. |
| Typography | Supported | Includes font size, line height modifiers, colors, alignment, transform, style, weight, families, tracking, text decoration, truncation, whitespace, and word-break utilities. |
| Visibility and interaction | Supported with limits | Includes overflow values supported by s&box, pointer events, documented cursor values plus arbitrary cursor values, z-index, and panel opacity utilities. |
| Transitions | Supported | Emits `transition`, `transition-*`, `duration-*`, `delay-*`, and `ease-*` declarations. |
| Effects | Supported with limits | Emits regular filters, drop-shadow filters, box shadow, text shadow, backdrop filter longhands, mix blend modes, animation declarations, transform, and transform-origin. Animation keyframes are not generated. |
| Important utilities | Supported | Supports both leading and trailing important syntax, such as `!p-4` and `p-4!`. |
| Slash modifiers | Supported with limits | Supports utility-specific modifiers such as background/text/border color opacity and text line height, for example `bg-accent/50`, `text-white/70`, `border-accent/50`, and `text-lg/7`. |
| Arbitrary values | Supported with limits | Emitted when the target utility can resolve the value and s&box supports the resulting property/value pair. |
| Arbitrary properties | Supported with limits | Emitted only when the property and value are allowed by the s&box capability checks. Unsupported properties are reported. |
| Pseudo variants | Supported | `hover:`, `active:`, `focus:`, `intro:`, and `outro:` emit generated pseudo selectors. |
| Browser selector variants | Parsed, not emitted | Selector variants such as `first:`, `last:`, `odd:`, `disabled:`, `before:`, `after:`, `group-*`, `peer-*`, `aria-*`, and `data-*` are detected and reported, but not emitted yet. |
| Grid layout | Unsupported | Grid utilities such as `grid` and grid-template arbitrary properties are reported instead of emitted. |
| Unsupported CSS properties or values | Reported | Classes that compile to properties or values outside the s&box support surface are skipped with stable diagnostics. |
