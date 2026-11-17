# Country outlier highlighting module

## Recommended personal contribution

Claim **FR6: country outlier detection and visual highlighting**. The design report defines an outlier as a value beyond 1.5 times the interquartile range from Q1 or Q3. The screenshots ask each member to take a concrete implementation module that can be shown and merged the following week. This module covers that bounded feature without depending on the speech/LLM service.

## Included implementation

`CountryOutlierHighlighter.cs` is a Unity component that:

- calculates Q1 and Q3 with the median-of-halves convention;
- ignores NaN and infinite values and requires at least four valid values;
- marks values strictly below `Q1 - 1.5 × IQR` or above `Q3 + 1.5 × IQR`;
- highlights each corresponding bar through a `MaterialPropertyBlock`, then restores its previous block when the view is reset or data is replaced.

## Integration contract

1. Add the script to the Unity project.
2. Add `CountryOutlierHighlighter` to a scene object.
3. From the chart builder, create an `OutlierBar` for each country using the currently selected metric and that country bar's `Renderer`.
4. Call `SetBars(...)` after the chart has been built or the height metric has changed.
5. Call `HighlightOutliers()` for the “find the outliers” action. Use the returned country names for an interpreted-action panel or a short spoken/text response.
6. Call `ResetHighlight()` when resetting the view. The module also clears its previous highlights when new bars are supplied.

The project source and existing chart-builder API were not included, so the adapter call in steps 3–4 must be connected to the team's actual bar/data types during integration.

## Merge-ready claim message

> I’ll take FR6, the outlier detection and highlighting module. I’ve implemented a Unity C# component that applies the report’s 1.5×IQR rule to the selected country metric, highlights the matching bars, and restores their original appearance on reset. I’ll connect it to the chart data and bring a working demo for us to merge next week.

## Contribution paragraph for the final report

> I implemented the country outlier-highlighting module for FR6. It calculates Q1 and Q3 from the active country metric, applies the 1.5×IQR fences, and highlights bars whose values fall outside those fences. The module uses per-renderer material property blocks so highlighting can be cleared without replacing the chart materials. During integration, it is connected to the chart builder's current metric and renderer references, and its output is checked against known values before the demonstration.

Keep the last sentence only after that integration and validation have actually been completed.

## Manual demonstration outline

Use at least four values with a clear outlier, for example `1, 2, 3, 4, 5, 6, 7, 8, 100`, and confirm that the high value's country bar is highlighted. Then reset the view and confirm the original bar color returns. Also try a dataset with no values outside the fences and a metric change followed by `SetBars(...)`.