# Application

Generation use cases and public contracts live here:

- `TailBoxGenerator` for in-memory generation.
- `TailBoxGenerationPipeline` for the staged generation flow.
- `TailBoxConfig`, `TailBoxSourceText`, and `TailBoxGenerationResult`.
- `TailBoxThemeFactory` for adapting public config into the domain theme model.
- `TailBoxScssRenderer` for deterministic generated stylesheet text.
- Public skipped-item reports with source file context.
- Razor source text class extraction.

This layer may orchestrate domain services, but it should not read or write project files directly.
