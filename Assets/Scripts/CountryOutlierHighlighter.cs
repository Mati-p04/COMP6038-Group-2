using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Finds Tukey outliers for the active country metric and highlights their bars.
/// Supply one OutlierBar per country after the chart has been built.
/// </summary>
public sealed class CountryOutlierHighlighter : MonoBehaviour
{
    [Serializable]
    public sealed class OutlierBar
    {
        public string countryName;
        public float value;
        public Renderer barRenderer;
    }

    [SerializeField] private Color outlierColor = new Color(1f, 0.25f, 0.1f, 1f);
    private readonly Dictionary<Renderer, MaterialPropertyBlock> originalBlocks =
        new Dictionary<Renderer, MaterialPropertyBlock>();
    private readonly List<string> lastOutlierNames = new List<string>();
    private List<OutlierBar> bars = new List<OutlierBar>();

    public IReadOnlyList<string> LastOutlierNames { get { return lastOutlierNames; } }

    /// <summary>Replace the country values/renderers, clearing any prior highlight.</summary>
    public void SetBars(IEnumerable<OutlierBar> newBars)
    {
        ResetHighlight();
        bars = newBars == null ? new List<OutlierBar>() : newBars.Where(b => b != null).ToList();
    }

    /// <summary>
    /// Uses Tukey fences from median-of-halves quartiles. Values exactly on a fence
    /// are not outliers; non-finite values are ignored.
    /// </summary>
    public List<string> HighlightOutliers()
    {
        ResetHighlight();
        List<OutlierBar> valid = bars.Where(b => !float.IsNaN(b.value) && !float.IsInfinity(b.value)).ToList();
        if (valid.Count < 4) return new List<string>();

        List<float> sorted = valid.Select(b => b.value).OrderBy(v => v).ToList();
        int midpoint = sorted.Count / 2;
        int upperStart = sorted.Count % 2 == 0 ? midpoint : midpoint + 1;
        float q1 = Median(sorted, 0, midpoint);
        float q3 = Median(sorted, upperStart, sorted.Count - upperStart);
        float iqr = q3 - q1;
        float lowerFence = q1 - 1.5f * iqr;
        float upperFence = q3 + 1.5f * iqr;

        foreach (OutlierBar bar in valid)
        {
            if (bar.value < lowerFence || bar.value > upperFence)
            {
                lastOutlierNames.Add(bar.countryName ?? string.Empty);
                ApplyHighlight(bar.barRenderer);
            }
        }
        return new List<string>(lastOutlierNames);
    }

    public void ResetHighlight()
    {
        foreach (KeyValuePair<Renderer, MaterialPropertyBlock> entry in originalBlocks)
            if (entry.Key != null) entry.Key.SetPropertyBlock(entry.Value);
        originalBlocks.Clear();
        lastOutlierNames.Clear();
    }

    private void ApplyHighlight(Renderer target)
    {
        if (target == null || originalBlocks.ContainsKey(target) || target.sharedMaterial == null) return;
        int colorProperty = GetColorProperty(target.sharedMaterial);
        if (colorProperty == 0) return;
        MaterialPropertyBlock original = new MaterialPropertyBlock();
        target.GetPropertyBlock(original);
        originalBlocks.Add(target, original);
        MaterialPropertyBlock highlighted = new MaterialPropertyBlock();
        target.GetPropertyBlock(highlighted);
        highlighted.SetColor(colorProperty, outlierColor);
        target.SetPropertyBlock(highlighted);
    }

    private static int GetColorProperty(Material material)
    {
        if (material.HasProperty("_BaseColor")) return Shader.PropertyToID("_BaseColor");
        if (material.HasProperty("_Color")) return Shader.PropertyToID("_Color");
        return 0;
    }

    private static float Median(IList<float> values, int start, int count)
    {
        int middle = start + count / 2;
        return count % 2 == 1 ? values[middle] : (values[middle - 1] + values[middle]) / 2f;
    }
}
