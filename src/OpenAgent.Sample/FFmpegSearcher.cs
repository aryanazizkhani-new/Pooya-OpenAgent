using System.Diagnostics;

namespace OpenAgent.Sample;

public static class FFmpegSearcher
{
    //--------------------------------------------------
    // Find All
    //--------------------------------------------------

    public static List<string> FindAll()
    {
        HashSet<string> result =
            new(StringComparer.OrdinalIgnoreCase);

        foreach (string file in SearchAll("ffmpeg.exe"))
        {
            if (!File.Exists(file))
                continue;

            FileInfo info =
                new(file);

            if (info.Length < 100000)
                continue;

            string path =
                file.ToLowerInvariant();

            if (path.Contains("$recycle.bin"))
                continue;

            if (path.Contains("system volume information"))
                continue;

            result.Add(file);
        }

        return result
            .OrderBy(x => x)
            .ToList();
    }

    //--------------------------------------------------
    // Find First
    //--------------------------------------------------

    public static string? FindSingle()
    {
        List<string> list =
            FindAll();

        if (list.Count == 0)
            return null;

        return list[0];
    }

    //--------------------------------------------------
    // Search
    //--------------------------------------------------

    private static IEnumerable<string> SearchAll(
        string pattern)
    {
        string es =
            Path.Combine(
                AppContext.BaseDirectory,
                "es.exe");

        if (!File.Exists(es))
            yield break;

        Process p = new();

        p.StartInfo.FileName =
            es;

        p.StartInfo.Arguments =
            pattern;

        p.StartInfo.RedirectStandardOutput =
            true;

        p.StartInfo.UseShellExecute =
            false;

        p.StartInfo.CreateNoWindow =
            true;

        p.Start();

        while (!p.StandardOutput.EndOfStream)
        {
            string? line =
                p.StandardOutput.ReadLine();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            line = line.Trim();

            if (!File.Exists(line))
                continue;

            if (!line.EndsWith(
                "ffmpeg.exe",
                StringComparison.OrdinalIgnoreCase))
                continue;

            yield return line;
        }

        p.WaitForExit();
    }
}