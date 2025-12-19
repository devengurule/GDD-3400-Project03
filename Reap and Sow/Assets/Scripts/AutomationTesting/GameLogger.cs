using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

// AI acknowledgement **Generative AI was used in helping to create this script**

/// <summary>
/// Structured CSV logger for automated tests.
/// CSV path: Assets/Editor/TestResults/<fileName>
/// Console: concise, human-readable lines.
/// </summary>
public class GameLogger
{
    private readonly List<string> lines = new List<string>();
    private readonly StringBuilder sb = new StringBuilder(256);

    private const string Header = "time,event,phase,x,y,angle,dist,attempt,totalAttempts,elapsed,note";

    private const string dir = "Game Test Results";

    // Toggle if you want console echo for each row
    public bool EchoToConsole { get; set; } = false;

    public GameLogger()
    {
        lines.Add(Header);
    }

    // Core row write
    private void Row(
        string evt, string phase,
        float? x = null, float? y = null,
        float? angle = null, float? dist = null,
        int? attempt = null, int? totalAttempts = null,
        float? elapsed = null, string note = null)
    {
        string t = Time.time.ToString("F2");

        string cell(string s) => Sanitize(s);
        string f(float? v) => v.HasValue ? v.Value.ToString(v == elapsed ? "F2" : (v == dist ? "F3" : "F2")) : "";
        string i(int? v) => v.HasValue ? v.Value.ToString() : "";

        string line = string.Join(",",
            cell(t),
            cell(evt),
            cell(phase),
            cell(f(x)),
            cell(f(y)),
            cell(f(angle)),
            cell(f(dist)),
            cell(i(attempt)),
            cell(i(totalAttempts)),
            cell(f(elapsed)),
            cell(note ?? "")
        );

        lines.Add(line);

        if (EchoToConsole)
        {
            // Short, scannable console message
            sb.Clear();
            sb.Append("[Boundary] ").Append(evt);
            if (!string.IsNullOrEmpty(phase)) sb.Append(" ").Append(phase);
            if (attempt.HasValue && totalAttempts.HasValue) sb.Append(" try ").Append(attempt.Value).Append("/").Append(totalAttempts.Value);
            if (angle.HasValue) sb.Append(" @ ").Append(angle.Value.ToString("F0")).Append("�");
            if (dist.HasValue) sb.Append(" moved=").Append(dist.Value.ToString("F3"));
            if (x.HasValue && y.HasValue) sb.Append(" pos=(").Append(x.Value.ToString("F2")).Append(", ").Append(y.Value.ToString("F2")).Append(")");
            if (elapsed.HasValue) sb.Append(" t=").Append(elapsed.Value.ToString("F2"));
            if (!string.IsNullOrEmpty(note)) sb.Append(" | ").Append(note);
            Debug.Log(sb.ToString());
        }
    }
    
    /// <summary>
    /// write row data from List of strings To CSV
    /// </summary>
    /// <param name="resultDataList"></param>
    private void Row(List<string> resultDataList)
    {
        string line = "";

        //for the string in resultDataList
        foreach (string item in resultDataList)
        {
            //If needed add a comma delimiter
            if (line != "")
            {
                line += ",";
            }

            //sanitize string (remove commas) using sanitize method and add to line
            line += Sanitize(item);
        }
        // append to CSV line
        lines.Add(line);
    }

    // Public helpers
    public void SessionStart(string testName, string settingsSummary)
        => Row("SessionStart", testName, note: settingsSummary);
    public void SessionStart(List<string> colNames)
    {
        Row(colNames);
    }

    public void Info(string phase, string note = null, Vector3? pos = null, float? elapsed = null)
        => Row("Info", phase, pos?.x, pos?.y, elapsed: elapsed, note: note);

    public void Warning(string phase, string note = null, Vector3? pos = null, float? elapsed = null)
        => Row("Warning", phase, pos?.x, pos?.y, elapsed: elapsed, note: note);

    public void Result(string status, string note = null, Vector3? pos = null, float? elapsed = null)
        => Row(status, "Result", pos?.x, pos?.y, elapsed: elapsed, note: note);

    // Public helpers
    public void Result(TestResult results)
    {
        Row(results.GetResultList());
    }
    public void SweepTry(int attempt, int total, float angleDeg, float movedDist, Vector3 pos)
        => Row("SweepTry", "RandomSweep", pos.x, pos.y, angleDeg, movedDist, attempt, total);

    public void SweepFreed(float angleDeg, float movedDist, Vector3 pos)
        => Row("Freed", "RandomSweep", pos.x, pos.y, angleDeg, movedDist, note: "freed=yes");

    public void BoundsEscaped(Vector3 pos)
        => Row("Escaped", "Boundary", pos.x, pos.y);

    public void Stuck(int minAttempts, Vector3 pos)
        => Row("Stuck", "Boundary", pos.x, pos.y, note: $"after>={minAttempts} tries");

    public void Timeout(Vector3 pos, float elapsed)
        => Row("Timeout", "Boundary", pos.x, pos.y, elapsed: elapsed);

    public void Write(string fileName = "TestLog.csv")
    {
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, fileName);

        using (var sw = new StreamWriter(path, false))
        {
            foreach (var l in lines) sw.WriteLine(l);
        }
        
        //Debug.Log($"[GameLogger] wrote: {path}");
    }

    private static string Sanitize(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";

        // Escape quotes by doubling them
        if (s.Contains('"'))
            s = s.Replace("\"", "\"\"");

        // If field contains comma, newline, or quote, wrap in quotes
        if (s.Contains(',') || s.Contains('\n') || s.Contains('\r') || s.Contains('"'))
            s = $"\"{s}\"";

        return s;
    }
}

