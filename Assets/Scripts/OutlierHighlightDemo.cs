using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Standalone sample chart proving the outlier component's data/render integration.
/// Replace the sample list and SetBars call with the project's chart data when available.
/// </summary>
public sealed class OutlierHighlightDemo : MonoBehaviour
{
    private sealed class CountryValue
    {
        public readonly string Name;
        public readonly float Value;
        public CountryValue(string name, float value) { Name = name; Value = value; }
    }

    private readonly CountryValue[] sample =
    {
        new CountryValue("Aster", 1), new CountryValue("Birch", 2),
        new CountryValue("Cedar", 3), new CountryValue("Dune", 4),
        new CountryValue("Elm", 5), new CountryValue("Flint", 6),
        new CountryValue("Grove", 7), new CountryValue("Harbor", 8),
        new CountryValue("Island", 100)
    };

    private CountryOutlierHighlighter highlighter;
    private Material generatedMaterial;
    private readonly List<GameObject> generatedObjects = new List<GameObject>();
    private string status = "Press H to highlight outliers, R to reset.";

    private void Start()
    {
        highlighter = gameObject.AddComponent<CountryOutlierHighlighter>();
        BuildSampleChart();
        FrameChartCamera();
    }

    private void BuildSampleChart()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null)
        {
            Debug.LogError("Outlier demo could not find a supported Lit/Standard shader.");
            return;
        }

        generatedMaterial = new Material(shader) { color = new Color(0.15f, 0.55f, 0.85f) };
        List<CountryOutlierHighlighter.OutlierBar> bars = new List<CountryOutlierHighlighter.OutlierBar>();
        for (int i = 0; i < sample.Length; i++)
        {
            CountryValue country = sample[i];
            float height = Mathf.Max(0.35f, Mathf.Log10(country.Value + 1f) * 2.4f);
            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.name = "Bar - " + country.Name;
            bar.transform.SetParent(transform, false);
            bar.transform.localPosition = new Vector3((i - (sample.Length - 1) / 2f) * 1.35f, height / 2f, 0f);
            bar.transform.localScale = new Vector3(0.82f, height, 0.82f);
            Renderer renderer = bar.GetComponent<Renderer>();
            renderer.sharedMaterial = generatedMaterial;
            generatedObjects.Add(bar);
            bars.Add(new CountryOutlierHighlighter.OutlierBar
            {
                countryName = country.Name,
                value = country.Value,
                barRenderer = renderer
            });
            CreateLabel(country, i);
        }
        highlighter.SetBars(bars);
        status = "Sample loaded: 1–8 and 100. Expected outlier: Island.";
    }

    private void CreateLabel(CountryValue country, int index)
    {
        GameObject label = new GameObject("Label - " + country.Name);
        label.transform.SetParent(transform, false);
        label.transform.localPosition = new Vector3((index - (sample.Length - 1) / 2f) * 1.35f, -0.48f, 0f);
        TextMesh text = label.AddComponent<TextMesh>();
        text.text = country.Name + "\n" + country.Value;
        text.fontSize = 42;
        text.characterSize = 0.075f;
        text.anchor = TextAnchor.UpperCenter;
        text.alignment = TextAlignment.Center;
        text.color = Color.white;
        generatedObjects.Add(label);
    }

    private void FrameChartCamera()
    {
        Camera camera = Camera.main;
        if (camera == null) return;
        camera.transform.position = new Vector3(0f, 5.8f, -20f);
        camera.transform.rotation = Quaternion.Euler(13f, 0f, 0f);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (keyboard.hKey.wasPressedThisFrame) Highlight();
        if (keyboard.rKey.wasPressedThisFrame) Reset();
    }

    private void Highlight()
    {
        if (highlighter == null) return;
        List<string> names = highlighter.HighlightOutliers();
        status = names.Count == 0 ? "No outliers found." : "Outliers: " + string.Join(", ", names.ToArray());
        Debug.Log(status);
    }

    private void Reset()
    {
        if (highlighter != null) highlighter.ResetHighlight();
        status = "Highlights cleared; original bar colors restored.";
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(16, 16, 440, 124), GUI.skin.box);
        GUILayout.Label("Country Outlier Demo — Tukey 1.5×IQR");
        GUILayout.Label(status);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Highlight (H)")) Highlight();
        if (GUILayout.Button("Reset (R)")) Reset();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void OnDestroy()
    {
        if (highlighter != null) highlighter.ResetHighlight();
        foreach (GameObject item in generatedObjects)
            if (item != null) Destroy(item);
        if (generatedMaterial != null) Destroy(generatedMaterial);
    }
}
