# Construction Dashboard Example

This example is intentionally stored as `.example` files so it does not compile as part of the tailw& library. Copy the files into a consuming s&box project and remove the `.example` suffixes.

Expected project layout after copying:

```text
tailwand.config.json
Code/
  ConstructionDashboard.razor
  ConstructionDashboard.razor.scss
```

Run `Editor > tailw& > Generate Now`, then open the panel in-game or in the editor. The stylesheet imports `/tailwand.generated.scss`, which is generated at `Code/tailwand.generated.scss` by the sample config.
