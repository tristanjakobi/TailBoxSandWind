# Infrastructure

s&box editor and project filesystem integration lives here:

- Editor menu actions.
- Watcher/debounce behavior.
- Config file load/save.
- Content glob discovery.
- `TailBoxGlobMatcher` for editor-side glob matching.
- Project-relative path filtering.
- Razor file reads and generated SCSS writes.

Convert files into application contracts such as `TailBoxSourceText` before calling the pure generator.
