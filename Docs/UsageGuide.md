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

## 5. Let the Watcher Regenerate

After initialization, the editor watcher is enabled. It watches configured content files and regenerates when Razor content changes.

Use this menu item if you want to pause or resume automatic generation:

```text
Editor > tailw& > Toggle Watcher
```

If styles look stale, run `Generate Now` once. This is also useful after changing `tailwand.config.json`.

## 6. Configure Tokens

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

## 7. Safelist Dynamic Classes

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

## 8. Use Arbitrary Values Carefully

Many arbitrary values are supported when they compile to s&box-safe properties:

```razor
<div class="w-[420px] bg-[#0d1418] shadow-[0_0_24px_rgba(0,0,0,0.45)]"></div>
<div class="[mask-scope:filter] [perspective-origin:50%_50%]"></div>
<div class="transform-[translateX(8px)_scale(1.05)]"></div>
```

Unsupported properties and values are skipped instead of emitted. This keeps generated output predictable and avoids stylesheet declarations that s&box cannot apply.

## 9. Check Unsupported Classes

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

## 10. Try the Demo and Example

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
