# Using tailw&

This guide walks through using tailw& in an s&box project. tailw& is a build-time helper: it scans Razor UI files for Tailwind-shaped utility classes and writes normal s&box-compatible SCSS. Your game loads the generated stylesheet like any other stylesheet.

## 1. Add the Library

Add this library to your s&box project, usually under `Libraries/TailBoxSandWind` or `Libraries/tailwand`.

The editor integration looks for Razor files in these places by default:

```json
[
  "Code/**/*.razor",
  "Libraries/tailwand/Code/**/*.razor",
  "Libraries/TailBoxSandWind/Code/**/*.razor"
]
```

You can change those paths later in `tailwand.config.json`.

## 2. Initialize tailw&

In the s&box editor, run:

```text
Editor > tailw& > Initialize
```

This creates `tailwand.config.json` in the project root if one does not already exist, generates the first stylesheet, and starts the editor watcher.

The default generated file is:

```text
Code/tailwand.generated.scss
```

If you already have an old `tailbox.config.json`, tailw& will still read it. New projects should use `tailwand.config.json`.

## 3. Import the Generated Stylesheet

Import the generated SCSS from a project stylesheet that your UI already loads:

```scss
@import "/tailwand.generated.scss";
```

The default output path is `Code/tailwand.generated.scss`, which s&box can address as `/tailwand.generated.scss`.

You can also import it from a component stylesheet:

```scss
@import "/tailwand.generated.scss";

.inventory-root {
  pointer-events: all;
}
```

Do not edit `tailwand.generated.scss` by hand. It is regenerated from your Razor files and config.

## 4. Use Utility Classes in Razor

Write classes directly on Razor panels and components:

```razor
<root class="absolute inset-0 flex flex-col gap-4 p-6 bg-bg text-text">
  <div class="rounded-lg border border-border bg-panel p-4 shadow-lg">
    <label class="text-xl font-bold text-accent">Inventory</label>
    <label class="mt-2 text-sm text-muted">12 items stored</label>
  </div>
</root>
```

Run:

```text
Editor > tailw& > Generate Now
```

tailw& scans the configured Razor files, finds the utility classes, and writes the matching SCSS.

## 5. Use Each Utility Family

Utilities are ordinary class names on Razor elements. Start with the role of the element, then add the utility families that describe its layout, size, spacing, color, and behavior.

### Screen Roots

Use a root element to cover the screen, set shared text styling, and establish the main layout direction.

```razor
<root class="absolute inset-0 flex flex-col bg-bg text-text font-sans pointer-events-all">
  ...
</root>
```

Common utilities:

| Need | Use |
| --- | --- |
| Fill the screen | `absolute inset-0` |
| Stack content vertically | `flex flex-col` |
| Put content in a row | `flex flex-row` |
| Allow mouse interaction | `pointer-events-all` |
| Set default colors | `bg-bg text-text` |

### Layout Containers

Use flex utilities for almost every s&box UI layout. Grid utilities are parsed, but they are reported instead of emitted.

```razor
<div class="flex flex-row flex-wrap items-center justify-between gap-4">
  <button class="px-4 py-2 rounded-md bg-accent text-black">Equip</button>
  <button class="px-4 py-2 rounded-md bg-panel border border-border">Drop</button>
</div>
```

Common utilities:

| Need | Use |
| --- | --- |
| Enable flex layout | `flex` |
| Direction | `flex-row`, `flex-col`, `flex-row-reverse`, `flex-col-reverse` |
| Wrapping | `flex-wrap`, `flex-nowrap`, `flex-wrap-reverse` |
| Main-axis alignment | `justify-start`, `justify-center`, `justify-between`, `justify-around`, `justify-evenly` |
| Cross-axis alignment | `items-start`, `items-center`, `items-end`, `items-stretch` |
| Individual alignment | `self-start`, `self-center`, `self-end`, `self-stretch` |
| Child sizing | `flex-1`, `flex-auto`, `flex-none`, `grow`, `shrink-0`, `basis-1/3`, `order-2` |

### Positioned Elements

Use position and inset utilities for overlays, anchored widgets, badges, and full-screen panels.

```razor
<div class="relative w-full h-[160px] bg-panel rounded-lg">
  <div class="absolute top-3 right-3 z-20 px-3 py-1 rounded bg-danger text-white">
    Alert
  </div>
</div>
```

Common utilities:

| Need | Use |
| --- | --- |
| Positioning mode | `static`, `relative`, `absolute` |
| Fill parent | `inset-0` |
| Anchor to sides | `top-3`, `right-3`, `bottom-3`, `left-3` |
| Axis insets | `inset-x-4`, `inset-y-2` |
| Negative offsets | `-top-2`, `-left-2` |
| Layer order | `z-0`, `z-10`, `z-50`, `z-[75]` |

### Size and Bounds

Use sizing utilities to define fixed, fractional, full, screen, min, max, and aspect-ratio bounds.

```razor
<div class="w-full max-w-[420px] min-h-[80px] p-4 rounded-lg bg-panel">
  <img class="w-full aspect-video rounded-md bg-[url(/ui/preview.png)] bg-cover bg-center" />
</div>
```

Common utilities:

| Need | Use |
| --- | --- |
| Width | `w-full`, `w-1/2`, `w-screen`, `w-[420px]` |
| Height | `h-full`, `h-screen`, `h-[160px]` |
| Square size | `size-4`, `size-10`, `size-[48px]` |
| Minimum bounds | `min-w-0`, `min-w-[220px]`, `min-h-[24px]` |
| Maximum bounds | `max-w-full`, `max-w-[1180px]`, `max-h-full` |
| Aspect ratio | `aspect-square`, `aspect-video`, `aspect-[4/3]` |

### Spacing

Use padding on the inside of a panel, margin outside the panel, and gap between flex children.

```razor
<div class="flex flex-col gap-3 p-5 mt-2 rounded-lg bg-panel">
  <label class="mb-2 text-sm text-muted">Inventory</label>
  <div class="flex flex-row gap-x-3 gap-y-2 flex-wrap">
    ...
  </div>
</div>
```

Common utilities:

| Need | Use |
| --- | --- |
| Padding | `p-4`, `px-4`, `py-2`, `pt-3`, `pb-[14px]` |
| Margin | `m-0`, `mt-2`, `mb-4`, `ml-auto` |
| Negative margin | `-mt-2`, `-ml-4` |
| Uniform child gap | `gap-4` |
| Axis child gap | `gap-x-3`, `gap-y-2` |

Negative padding is not supported. Tailwind `space-x-*` and `space-y-*` utilities require child selectors, so use `gap-*` instead.

### Text

Use text utilities for color, size, line height, alignment, weight, family, casing, tracking, decoration, truncation, and wrapping.

```razor
<div class="flex flex-col gap-1">
  <label class="text-xs uppercase tracking-wider text-muted">Status</label>
  <label class="text-2xl leading-tight font-black text-accent">Online</label>
  <label class="text-sm leading-relaxed text-white/70 truncate">Connected to the match server</label>
</div>
```

Common utilities:

| Need | Use |
| --- | --- |
| Color | `text-text`, `text-muted`, `text-accent`, `text-white/70`, `text-[#0d1418]` |
| Size | `text-xs`, `text-sm`, `text-lg`, `text-3xl`, `text-[34px]` |
| Line height | `leading-none`, `leading-tight`, `leading-relaxed`, `text-lg/7`, `leading-[42px]` |
| Alignment | `text-left`, `text-center`, `text-right` |
| Weight and family | `font-sans`, `font-mono`, `font-bold`, `font-black` |
| Style and casing | `italic`, `not-italic`, `uppercase`, `lowercase`, `capitalize`, `normal-case` |
| Tracking | `tracking-normal`, `tracking-wide`, `tracking-[0.18em]` |
| Decoration | `underline`, `line-through`, `decoration-accent`, `decoration-2`, `underline-offset-4` |
| Overflow text | `truncate`, `whitespace-nowrap`, `whitespace-pre-line`, `break-all` |

### Backgrounds and Images

Use `bg-*` for theme colors, arbitrary colors, image URLs, image sizing, image position, and repeat behavior.

```razor
<div class="bg-panel border border-border rounded-lg p-4">
  <div class="h-[160px] rounded-md bg-[url(/ui/map.png)] bg-cover bg-center bg-no-repeat"></div>
</div>
```

Common utilities:

| Need | Use |
| --- | --- |
| Theme color | `bg-panel`, `bg-accent`, `bg-danger`, `bg-good` |
| Opacity modifier | `bg-white/10`, `bg-accent/50` |
| Arbitrary color | `bg-[#101820]`, `bg-[color:#1b2730]` |
| Image | `bg-[url(/ui/panel.png)]` |
| Image size | `bg-cover`, `bg-contain`, `bg-auto` |
| Position | `bg-center`, `bg-top`, `bg-right`, `bg-bottom`, `bg-left` |
| Repeat | `bg-no-repeat`, `bg-repeat`, `bg-repeat-x`, `bg-repeat-y` |

Tailwind gradient stop utilities such as `from-*`, `via-*`, and `to-*` are not emitted. Use a direct arbitrary background image instead, such as `bg-[linear-gradient(red,_blue)]`.

### Borders and Radius

Use border utilities for card edges, separators, active states, and rounded UI surfaces.

```razor
<div class="border border-border rounded-lg bg-panel p-4">
  <div class="border-l-[8px] border-l-accent pl-4">
    Highlighted row
  </div>
</div>
```

Common utilities:

| Need | Use |
| --- | --- |
| Border width | `border`, `border-0`, `border-2`, `border-[3px]` |
| Side border | `border-t`, `border-r`, `border-b`, `border-l` |
| Side width | `border-t-[8px]`, `border-l-[8px]` |
| Border color | `border-border`, `border-accent`, `border-white/10` |
| Side color | `border-t-accent`, `border-l-accent` |
| Radius | `rounded`, `rounded-sm`, `rounded-md`, `rounded-lg`, `rounded-xl`, `rounded-full`, `rounded-[18px]` |
| Exact corners | `[border-top-left-radius:18px]`, `[border-bottom-right-radius:18px]` |

### Overflow, Pointer, and Cursor

Use overflow utilities for clipped panels and scroll areas. Use pointer and cursor utilities on interactive panels.

```razor
<div class="h-[220px] overflow-y-scroll pointer-events-all cursor-pointer">
  ...
</div>
```

Common utilities:

| Need | Use |
| --- | --- |
| Overflow | `overflow-hidden`, `overflow-visible`, `overflow-scroll` |
| Axis overflow | `overflow-x-hidden`, `overflow-x-scroll`, `overflow-y-scroll` |
| Pointer events | `pointer-events-none`, `pointer-events-auto`, `pointer-events-all` |
| Cursor | `cursor-pointer`, `cursor-text`, `cursor-not-allowed`, `cursor-wait`, `cursor-crosshair`, `cursor-[crosshair]` |

### Effects

Use effect utilities for opacity, shadows, text shadows, filters, backdrop filters, and blend modes.

```razor
<div class="opacity-75 shadow-lg backdrop-blur-lg bg-white/10 rounded-lg p-4">
  <label class="text-shadow-lg text-2xl font-black">Glass Panel</label>
</div>
```

Common utilities:

| Need | Use |
| --- | --- |
| Opacity | `opacity-75`, `opacity-[0.35]` |
| Box shadow | `shadow`, `shadow-lg`, `shadow-none`, `shadow-[0_0_12px_black]` |
| Text shadow | `text-shadow`, `text-shadow-lg`, `text-shadow-[0_0_12px_black]` |
| Filters | `blur-sm`, `brightness-125`, `contrast-125`, `grayscale`, `invert`, `hue-rotate-15`, `saturate-150`, `sepia`, `drop-shadow-lg` |
| Backdrop filters | `backdrop-blur-lg`, `backdrop-brightness-125`, `backdrop-contrast-125`, `backdrop-hue-rotate-15`, `backdrop-invert`, `backdrop-saturate-150`, `backdrop-sepia` |
| Blend mode | `mix-blend-normal`, `mix-blend-multiply` |

### Motion

Use transition utilities for state changes, direct transform output for transforms, and animation utilities for declarations. Define keyframes in project SCSS.

```razor
<button class="transition-colors duration-150 ease-in-out hover:bg-accent active:bg-accent-dark">
  Start
</button>

<div class="transform-[translateX(4px)_scale(1.1)] origin-top-left animate-[fade_1s_ease]"></div>
```

Common utilities:

| Need | Use |
| --- | --- |
| Transition property | `transition`, `transition-all`, `transition-colors`, `transition-opacity`, `transition-shadow`, `transition-transform` |
| Timing | `duration-150`, `delay-75`, `ease-in-out` |
| Transform | `transform-none`, `transform-[translateX(4px)_scale(1.1)]` |
| Transform origin | `origin-center`, `origin-top-left`, `origin-bottom-right` |
| Animation | `animate-none`, `animate-[fade_1s_ease]`, `[animation-duration:1s]`, `[animation-name:fade]` |

Tailwind `rotate-*`, `scale-*`, `translate-*`, and `skew-*` depend on CSS variables, so use `transform-[...]` when you want direct s&box transform output.

### State Variants

Prefix a supported utility with an s&box pseudo variant when the style should apply only in that state.

```razor
<button class="bg-panel border border-border hover:bg-accent active:bg-accent-dark focus:border-accent transition-colors">
  Select
</button>

<div class="intro:opacity-0 outro:opacity-0 transition-opacity duration-150"></div>
```

Supported emitted variants are `hover:`, `active:`, `focus:`, `intro:`, and `outro:`.

Browser selector variants such as `disabled:`, `first:`, `group-hover:`, `peer-*`, `aria-*`, and `data-*` are reported instead of emitted. For those states, switch classes from component code.

### Arbitrary Values and Properties

Use arbitrary values when the utility family is supported but you need a one-off value. Use arbitrary properties when s&box supports the property and value directly.

```razor
<div class="w-[420px] bg-[#0d1418] shadow-[0_0_24px_rgba(0,0,0,0.45)]"></div>
<div class="[mask-scope:filter] [perspective-origin:50%_50%]"></div>
```

Underscores inside arbitrary values become spaces, which keeps class names readable in Razor: `transform-[translateX(8px)_scale(1.05)]`.

CSS custom properties such as `[--brand:#fff]` are skipped because variable resolution is not part of the verified s&box subset.

## 6. Let the Watcher Regenerate

After initialization, the editor watcher is enabled. It watches configured content files and regenerates when Razor content changes.

Use this menu item if you want to pause or resume automatic generation:

```text
Editor > tailw& > Toggle Watcher
```

If styles look stale, run `Generate Now` once. This is also useful after changing `tailwand.config.json`.

## 7. Configure Tokens

The config merges your values with tailw& defaults, so you only need to add or override the tokens your project cares about.

Example:

```json
{
  "outputPath": "Code/tailwand.generated.scss",
  "content": [
    "Code/**/*.razor",
    "Libraries/TailBoxSandWind/Code/**/*.razor"
  ],
  "colors": {
    "brand": "#5cc8ff",
    "brand-dark": "#1a5f7a",
    "panel": "rgba( 20, 24, 28, 0.94 )"
  },
  "spacing": {
    "18": "72px"
  },
  "radii": {
    "2xl": "20px"
  },
  "animations": {
    "pop": "pop 0.18s ease-out both"
  }
}
```

Then use those tokens in Razor:

```razor
<button class="rounded-2xl bg-brand px-18 py-4 text-black hover:bg-brand-dark animate-pop">
  Start
</button>
```

Named animations emit the `animation` declaration only. Define the matching `@keyframes` in your own project SCSS.

## 8. Safelist Dynamic Classes

tailw& can find static classes easily, and it also scans Razor string literals for conditional classes. If a class is assembled dynamically and does not appear as a complete string, safelist it.

Safelist in config:

```json
{
  "safelist": [
    "bg-good",
    "bg-danger",
    "text-white/70",
    "z-50"
  ]
}
```

Or safelist near the component that needs it:

```razor
@* tailw& safelist: bg-good bg-danger text-white/70 z-50 *@
```

These comment prefixes also work:

```razor
@* tailwand safelist: bg-accent *@
@* tailbox safelist: bg-accent *@
```

## 9. Use Arbitrary Values Carefully

Many arbitrary values are supported when they compile to s&box-safe properties:

```razor
<div class="w-[420px] bg-[#0d1418] shadow-[0_0_24px_rgba(0,0,0,0.45)]"></div>
<div class="[mask-scope:filter] [perspective-origin:50%_50%]"></div>
<div class="transform-[translateX(8px)_scale(1.05)]"></div>
```

Unsupported properties and values are skipped instead of emitted. This keeps generated output predictable and avoids stylesheet declarations that s&box cannot apply.

## 10. Check Unsupported Classes

After `Initialize` or `Generate Now`, the editor dialog shows how many classes were generated, skipped, and warned about.

Skipped classes usually mean one of these things:

- The utility depends on browser CSS behavior that s&box does not support.
- The utility needs Tailwind CSS variables, such as gradient stop utilities.
- The property or value failed the s&box style capability check.
- The class was not a real utility and was picked up from a broad Razor string scan.

Common alternatives:

| Instead of | Use |
| --- | --- |
| `grid`, `grid-cols-*` | Flex layout utilities |
| `from-*`, `via-*`, `to-*` | `bg-[linear-gradient(...)]` |
| `rotate-*`, `scale-*`, `translate-*` | `transform-[...]` |
| Browser selector variants like `disabled:` or `group-hover:` | Component state plus ordinary classes |
| CSS variable utilities like `[--brand:#fff]` | Config tokens or direct supported properties |

For the full support surface, see `Docs/StyleProperties.md`.

## 11. Try the Demo and Example

To see the supported utilities in action, add `TailBoxDemo` to a GameObject. It creates or reuses the needed screen setup and opens the bundled showcase menu.

The construction dashboard example lives in:

```text
Examples/ConstructionDashboard
```

Those files end in `.example` so they do not compile as part of the library. Copy them into a consuming project, remove the `.example` suffixes, then run:

```text
Editor > tailw& > Generate Now
```

## Typical Workflow

1. Write UI in Razor using utility classes.
2. Save the Razor file.
3. Let the watcher regenerate, or run `Generate Now`.
4. Keep project-specific tokens in `tailwand.config.json`.
5. Safelist classes that are built dynamically.
6. Put custom SCSS, keyframes, and component-specific selectors in your own stylesheets.

tailw& should own generated utility output. Your project should own hand-written styles, assets, keyframes, and component behavior.
