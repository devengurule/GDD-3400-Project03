#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UEvent = UnityEngine.Event;
using UEvtType = UnityEngine.EventType;

// AI acknowledgement **Lots of generative AI was used in creating this script to create the heatmap**


/// <summary>
/// Editor window to visualize BoundaryTest CSV logs:
/// - Heatmap of player positions
/// - Path polyline
/// - Markers for Escaped (red X) and Stuck (yellow warning)
/// - Optional bounds from SessionStart note or auto-fit
/// </summary>
public class BoundaryLogVisualizer : EditorWindow
{
    // --- UI state ---
    [SerializeField]
    private string csvPath =
        "Assets/AutomationTesting/Editor/TestResults/BoundaryTest.csv";

    [SerializeField] private int gridResolution = 128;     // heatmap grid size (square)
    [SerializeField] private bool logScale = true;         // makes dense areas pop without washing out
    [SerializeField] private float clampMin = 0f;          // optional value clamp (after scaling)
    [SerializeField] private float clampMax = 1f;

    [SerializeField] private bool autoDetectBounds = true; // use min/max of data if true
    [SerializeField] private Rect manualBounds = new Rect(-8, -4.5f, 16, 9); // x,y,width,height

    [SerializeField] private bool drawPath = true;
    [SerializeField] private bool drawHeatmap = true;
    [SerializeField] private bool drawMarkers = true;
    [SerializeField] private bool drawBounds = true;

    private Texture2D heatmapTex;
    private Rect dataBounds;                // world rect used to map x,y to texture
    private List<Vector2> path = new List<Vector2>();
    private List<Vector2> escapedPoints = new List<Vector2>();
    private List<Vector2> stuckPoints = new List<Vector2>();

    private bool loaded;
    private string status = "Load a CSV to visualize.";

    // SessionStart bounds parser, looks for: bounds=(minX,minY)–(maxX,maxY)
    static readonly Regex boundsRegex = new Regex(
        @"bounds=\(([-+]?\d*\.?\d+),\s*([-+]?\d*\.?\d+)\)[\u2013\-–]\(([-+]?\d*\.?\d+),\s*([-+]?\d*\.?\d+)\)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    [MenuItem("Tools/Automation/Boundary Log Visualizer")]
    public static void ShowWindow()
    {
        var w = GetWindow<BoundaryLogVisualizer>("Boundary Visualizer");
        w.minSize = new Vector2(520, 480);
        w.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Boundary Test – CSV Visualizer", EditorStyles.boldLabel);

        EditorGUILayout.Space(2);
        EditorGUI.BeginChangeCheck();
        csvPath = EditorGUILayout.TextField("CSV Path", csvPath);
        gridResolution = Mathf.Clamp(EditorGUILayout.IntField("Grid Resolution", gridResolution), 16, 1024);
        logScale = EditorGUILayout.Toggle("Log Scale", logScale);
        clampMin = EditorGUILayout.Slider("Clamp Min", clampMin, 0f, 1f);
        clampMax = EditorGUILayout.Slider("Clamp Max", clampMax, 0f, 1f);

        EditorGUILayout.Space(4);
        autoDetectBounds = EditorGUILayout.ToggleLeft("Auto-detect bounds from data", autoDetectBounds);
        if (!autoDetectBounds)
        {
            manualBounds = EditorGUILayout.RectField(new GUIContent("Manual Bounds (x,y,w,h)"), manualBounds);
        }

        EditorGUILayout.Space(4);
        drawHeatmap = EditorGUILayout.ToggleLeft("Draw Heatmap", drawHeatmap);
        drawPath = EditorGUILayout.ToggleLeft("Draw Path Polyline", drawPath);
        drawMarkers = EditorGUILayout.ToggleLeft("Draw Markers (Escaped/Stuck)", drawMarkers);
        drawBounds = EditorGUILayout.ToggleLeft("Draw Bounds Rect", drawBounds);

        EditorGUILayout.Space(6);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Load / Rebuild", GUILayout.Height(26)))
        {
            LoadAndBuild();
        }
        GUI.enabled = loaded && heatmapTex != null;
        if (GUILayout.Button("Save Heatmap PNG", GUILayout.Height(26)))
        {
            SaveHeatmapPng();
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6);
        EditorGUILayout.HelpBox(status, MessageType.Info);

        // Draw preview
        GUILayout.FlexibleSpace();
        var rect = GUILayoutUtility.GetRect(position.width - 24, position.height * 0.45f);
        DrawPreview(rect);
    }

    private void LoadAndBuild()
    {
        loaded = false;
        path.Clear();
        escapedPoints.Clear();
        stuckPoints.Clear();
        DestroyHeatmapTex();

        if (string.IsNullOrEmpty(csvPath) || !File.Exists(csvPath))
        {
            status = "CSV not found. Check the path.";
            return;
        }

        try
        {
            var rows = File.ReadAllLines(csvPath);
            if (rows.Length <= 1)
            {
                status = "CSV has no data rows.";
                return;
            }

            // Parse header
            var header = rows[0].Trim();
            // Expected: time,event,phase,x,y,angle,dist,attempt,totalAttempts,elapsed,note

            // Collect positions and special events
            var positions = new List<Vector2>();
            var xs = new List<float>();
            var ys = new List<float>();
            Rect? sessionBounds = null;

            for (int i = 1; i < rows.Length; i++)
            {
                var line = rows[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cols = SplitCsv(line);

                // Safe read helpers
                string Get(string name, int idx) => idx < cols.Length ? cols[idx] : "";

                // Column indices based on the header we write
                // 0=time 1=event 2=phase 3=x 4=y 5=angle 6=dist 7=attempt 8=totalAttempts 9=elapsed 10=note
                string evt = Get("event", 1);
                string phase = Get("phase", 2);
                string sx = Get("x", 3);
                string sy = Get("y", 4);
                string note = Get("note", 10);

                // Try parse x,y for path/heatmap
                if (TryParseFloat(sx, out float x) && TryParseFloat(sy, out float y))
                {
                    var p = new Vector2(x, y);
                    positions.Add(p);
                    xs.Add(x);
                    ys.Add(y);

                    // Special markers
                    if (evt == "Escaped")
                        escapedPoints.Add(p);
                    else if (evt == "Stuck")
                        stuckPoints.Add(p);
                }

                // Try parse bounds from SessionStart note once
                if (sessionBounds == null && evt == "SessionStart" && !string.IsNullOrEmpty(note))
                {
                    var m = boundsRegex.Match(note);
                    if (m.Success)
                    {
                        float minX = float.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                        float minY = float.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                        float maxX = float.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture);
                        float maxY = float.Parse(m.Groups[4].Value, CultureInfo.InvariantCulture);
                        sessionBounds = RectMinMax(minX, minY, maxX, maxY);
                    }
                }
            }

            if (positions.Count == 0)
            {
                status = "No (x,y) positions in CSV.";
                return;
            }

            path = positions;

            // Bounds
            if (autoDetectBounds)
            {
                // Prefer session bounds if present, otherwise fit to data with padding
                if (sessionBounds.HasValue)
                {
                    dataBounds = sessionBounds.Value;
                }
                else
                {
                    float minX = Min(xs), maxX = Max(xs);
                    float minY = Min(ys), maxY = Max(ys);
                    var r = RectMinMax(minX, minY, maxX, maxY);
                    dataBounds = Expand(r, 0.05f); // +5% padding
                }
            }
            else
            {
                dataBounds = manualBounds;
            }

            // Build heatmap
            if (drawHeatmap)
                heatmapTex = BuildHeatmapTexture(path, dataBounds, gridResolution, logScale, clampMin, clampMax);
            else
                heatmapTex = null;

            loaded = true;
            status = $"Loaded {positions.Count} positions. Escaped: {escapedPoints.Count}, Stuck: {stuckPoints.Count}.";
            Repaint();
        }
        catch (Exception ex)
        {
            status = "Error reading CSV: " + ex.Message;
        }
    }

    private void DrawPreview(Rect rect)
    {
        if (UEvent.current.type != UEvtType.Repaint) return;

        // Background
        EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1f));

        if (!loaded) return;

        // Draw heatmap
        if (drawHeatmap && heatmapTex != null)
        {
            var imgRect = FitAspect(rect, heatmapTex.width, heatmapTex.height);
            GUI.DrawTexture(imgRect, heatmapTex, ScaleMode.ScaleToFit, false);
            // Overlay everything else in the same rect space
            DrawOverlay(imgRect);
        }
        else
        {
            // No heatmap: we still draw overlays on an empty canvas
            var imgRect = rect; // just use full rect
            DrawOverlay(imgRect);
        }
    }

    private void DrawOverlay(Rect imgRect)
    {
        if (path.Count == 0) return;

        // World->screen mapping func
        Vector2 ToScreen(Vector2 world)
        {
            float u = Mathf.InverseLerp(dataBounds.xMin, dataBounds.xMax, world.x);
            float v = Mathf.InverseLerp(dataBounds.yMin, dataBounds.yMax, world.y);
            return new Vector2(Mathf.Lerp(imgRect.xMin, imgRect.xMax, u),
                               Mathf.Lerp(imgRect.yMax, imgRect.yMin, v)); // y inverted for GUI
        }

        // Bounds rect
        if (drawBounds)
        {
            Handles.color = new Color(1f, 1f, 1f, 0.6f);
            var p00 = ToScreen(new Vector2(dataBounds.xMin, dataBounds.yMin));
            var p10 = ToScreen(new Vector2(dataBounds.xMax, dataBounds.yMin));
            var p11 = ToScreen(new Vector2(dataBounds.xMax, dataBounds.yMax));
            var p01 = ToScreen(new Vector2(dataBounds.xMin, dataBounds.yMax));
            Handles.DrawAAPolyLine(2f, p00, p10, p11, p01, p00);
        }

        // Path polyline
        if (drawPath && path.Count > 1)
        {
            Handles.color = new Color(0.2f, 0.8f, 1f, 0.9f);
            var pts = new Vector3[path.Count];
            for (int i = 0; i < path.Count; i++) pts[i] = ToScreen(path[i]);
            Handles.DrawAAPolyLine(1.5f, pts);
        }

        // Markers
        if (drawMarkers)
        {
            // Escaped = red X
            Handles.color = new Color(1f, 0.2f, 0.2f, 0.95f);
            foreach (var p in escapedPoints)
                DrawX(ToScreen(p), 8f);

            // Stuck = yellow diamond
            Handles.color = new Color(1f, 0.85f, 0.2f, 0.95f);
            foreach (var p in stuckPoints)
                DrawDiamond(ToScreen(p), 7f);
        }
    }

    private Texture2D BuildHeatmapTexture(List<Vector2> points, Rect bounds, int res, bool useLog, float cMin, float cMax)
    {
        // 1) Bin counts
        int w = res, h = res;
        var bins = new float[w * h];
        float maxCount = 0f;

        foreach (var p in points)
        {
            int ix = (int)Mathf.Floor(Mathf.InverseLerp(bounds.xMin, bounds.xMax, p.x) * (w - 1));
            int iy = (int)Mathf.Floor(Mathf.InverseLerp(bounds.yMin, bounds.yMax, p.y) * (h - 1));
            if (ix < 0 || ix >= w || iy < 0 || iy >= h) continue;
            int idx = iy * w + ix;
            bins[idx] += 1f;
            if (bins[idx] > maxCount) maxCount = bins[idx];
        }

        if (maxCount <= 0f) maxCount = 1f;

        // 2) Normalize + optional log scale
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int iy = 0; iy < h; iy++)
        {
            for (int ix = 0; ix < w; ix++)
            {
                int idx = iy * w + ix;
                float v = bins[idx] / maxCount;

                if (useLog)
                {
                    // log scale to make sparse areas visible:
                    // log(1 + 9*v) maps v in [0..1] to [0..log(10)]
                    v = Mathf.Log(1f + 9f * v, 10f);
                }

                // clamp after scaling
                v = Mathf.InverseLerp(cMin, cMax, v);
                v = Mathf.Clamp01(v);

                Color c = HeatColor(v); // simple blue->red gradient
                tex.SetPixel(ix, iy, c);
            }
        }

        tex.Apply();
        return tex;
    }

    // Simple heat gradient: 0=deep blue, 0.5=yellow, 1=red
    private static Color HeatColor(float t)
    {
        t = Mathf.Clamp01(t);
        // blend through blue -> cyan -> green -> yellow -> red
        if (t < 0.25f) return Color.Lerp(new Color(0.05f, 0.10f, 0.6f), new Color(0.1f, 0.8f, 1f), t / 0.25f);
        if (t < 0.50f) return Color.Lerp(new Color(0.1f, 0.8f, 1f), new Color(0.1f, 1f, 0.1f), (t - 0.25f) / 0.25f);
        if (t < 0.75f) return Color.Lerp(new Color(0.1f, 1f, 0.1f), new Color(1f, 1f, 0.1f), (t - 0.50f) / 0.25f);
        return Color.Lerp(new Color(1f, 1f, 0.1f), new Color(1f, 0.1f, 0.1f), (t - 0.75f) / 0.25f);
    }

    private void SaveHeatmapPng()
    {
        if (heatmapTex == null)
        {
            status = "No heatmap to save.";
            return;
        }

        string dir = Path.GetDirectoryName(csvPath)?.Replace('\\', '/');
        if (string.IsNullOrEmpty(dir)) dir = "Assets";

        string fileName = Path.GetFileNameWithoutExtension(csvPath) + "_heatmap.png";
        string outPath = Path.Combine(dir, fileName);

        try
        {
            var bytes = heatmapTex.EncodeToPNG();
            File.WriteAllBytes(outPath, bytes);
            AssetDatabase.Refresh();
            status = $"Saved heatmap PNG: {outPath}";
        }
        catch (Exception ex)
        {
            status = "Failed to save PNG: " + ex.Message;
        }
    }

    private static string[] SplitCsv(string line)
    {
        // Our CSV is simple (no quoted commas), since we sanitize commas in notes.
        return line.Split(',');
    }

    private static bool TryParseFloat(string s, out float v)
    {
        return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v);
    }

    private static float Min(List<float> a)
    {
        float m = float.PositiveInfinity;
        for (int i = 0; i < a.Count; i++) if (a[i] < m) m = a[i];
        return float.IsInfinity(m) ? 0f : m;
    }

    private static float Max(List<float> a)
    {
        float m = float.NegativeInfinity;
        for (int i = 0; i < a.Count; i++) if (a[i] > m) m = a[i];
        return float.IsInfinity(m) ? 1f : m;
    }

    private static Rect RectMinMax(float minX, float minY, float maxX, float maxY)
    {
        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    private static Rect Expand(Rect r, float pct)
    {
        float dx = r.width * pct;
        float dy = r.height * pct;
        return new Rect(r.x - dx, r.y - dy, r.width + 2 * dx, r.height + 2 * dy);
    }

    private static void DrawX(Vector2 p, float size)
    {
        Vector2 a = p + new Vector2(-size, -size);
        Vector2 b = p + new Vector2(size, size);
        Vector2 c = p + new Vector2(-size, size);
        Vector2 d = p + new Vector2(size, -size);
        Handles.DrawAAPolyLine(2f, a, b);
        Handles.DrawAAPolyLine(2f, c, d);
    }

    private static void DrawDiamond(Vector2 p, float size)
    {
        Vector3 a = p + new Vector2(0, -size);
        Vector3 b = p + new Vector2(size, 0);
        Vector3 c = p + new Vector2(0, size);
        Vector3 d = p + new Vector2(-size, 0);
        Handles.DrawAAPolyLine(2f, a, b, c, d, a);
    }

    /// <summary>
    /// Fit a width/height into a target rect while preserving aspect ratio.
    /// Returns the new rect.
    /// </summary>
    private static Rect FitAspect(Rect target, int texWidth, int texHeight)
    {
        float targetAspect = target.width / target.height;
        float texAspect = (float)texWidth / texHeight;

        if (texAspect > targetAspect)
        {
            // Fit by width
            float height = target.width / texAspect;
            float y = target.y + (target.height - height) * 0.5f;
            return new Rect(target.x, y, target.width, height);
        }
        else
        {
            // Fit by height
            float width = target.height * texAspect;
            float x = target.x + (target.width - width) * 0.5f;
            return new Rect(x, target.y, width, target.height);
        }
    }


    private void DestroyHeatmapTex()
    {
        if (heatmapTex != null)
        {
            DestroyImmediate(heatmapTex);
            heatmapTex = null;
        }
    }

    private void OnDisable()
    {
        DestroyHeatmapTex();
    }
}
#endif
