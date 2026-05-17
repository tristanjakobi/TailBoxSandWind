# Style Property Coverage

tailw& emits s&box-compatible SCSS for a verified style-property catalog. Support means either a Tailwind-shaped utility compiles to the property, or arbitrary property syntax compiles directly:

```razor
<div class="shadow-lg opacity-75 [mask-scope:filter] [perspective-origin:50%_50%]"></div>
```

## Supported Surface

The full documented s&box style catalog is accepted by arbitrary property generation, including custom properties such as `background-image-tint`, `border-image-tint`, `mask-scope`, `sound-in`, `sound-out`, and `text-stroke`.

Recently unlocked groups:

| Group | Examples |
| --- | --- |
| Bounds and aspect | `min-w-0`, `min-h-[24px]`, `aspect-square`, `aspect-[4/3]` |
| Background images | `bg-[url(/ui/panel.png)]`, `bg-cover`, `bg-center`, `bg-no-repeat` |
| Opacity and blend | `opacity-75`, `opacity-[0.35]`, `mix-blend-multiply` |
| Shadows | `shadow`, `shadow-lg`, `shadow-[0_0_12px_black]`, `text-shadow-lg` |
| Backdrop filters | `backdrop-blur-lg`, `backdrop-brightness-125`, `backdrop-hue-rotate-15` |
| Motion declarations | `transition`, `transition-opacity`, `duration-150`, `delay-75`, `ease-in-out`, `transform-none`, `transform-[translateX(4px)_scale(1.1)]`, `origin-top-left` |
| Animation declarations | `animate-none`, `animate-[fade_1s_ease]`, `[animation-duration:1s]` |
| Text decoration | `underline-offset-4`, `-underline-offset-2` |
| Cursor values | `cursor-none`, `cursor-progress`, `cursor-wait`, `cursor-crosshair`, `cursor-move`, `cursor-[crosshair]` |
| s&box pseudo variants | `hover:bg-accent`, `active:bg-accent`, `focus:border-accent`, `intro:opacity-0`, `outro:opacity-0` |

## Important Limits

tailw& still avoids features that require browser CSS machinery or project-specific runtime assets:

| Area | Status |
| --- | --- |
| CSS variables/custom properties | `[--brand-color:#fff]` is still skipped because variable resolution is not part of the verified s&box subset. |
| Grid/table/browser layout | `grid`, table display values, and grid template properties remain report-only. |
| Browser selector variants | `first:`, `last:`, `disabled:`, `group-*`, `peer-*`, `aria-*`, and `data-*` are still report-only. |
| Tailwind gradient stop utilities | `bg-gradient-*`, `from-*`, `via-*`, and `to-*` remain report-only because they depend on generated CSS variables. Use direct arbitrary image values such as `bg-[linear-gradient(red,_blue)]`. |
| Compositional transform utilities | `rotate-*`, `scale-*`, `translate-*`, and `skew-*` remain report-only because Tailwind normally composes them through CSS variables. Use `transform-[...]` for direct s&box transform output. |
| Keyframes | Animation declarations are emitted, but keyframes are not generated. Define `@keyframes` in project SCSS when using named animations. |

## Arbitrary Property Examples

```razor
<div class="[border-top-left-radius:18px] [border-bottom-right-radius:18px]"></div>
<div class="[backdrop-filter:blur(10px)] [mask-scope:filter]"></div>
<div class="[animation-name:fade] [animation-duration:1s] [animation-fill-mode:both]"></div>
```

Unsupported properties or values are still reported with deterministic diagnostics so generation output stays predictable.
