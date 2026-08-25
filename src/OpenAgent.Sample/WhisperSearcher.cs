using System.Diagnostics;

namespace OpenAgent.Sample;

public static class WhisperSearcher
{
public static string? FindFFmpeg()
{
    return Search("ffmpeg.exe");
}
    public static string? FindExecutable()
    {
        return Search("whisper-cli.exe");
    }

    public static List<string> FindModels()
    {
        List<string> models = new();

        foreach (string file in SearchAll("ggml-*.bin"))
        {
            models.Add(file);
        }

        return models;
    }
    public static List<string> FindMicrophones()
{
    List<string> devices = new();

    Process p = new();

    p.StartInfo.FileName = FindFFmpeg()!;

    p.StartInfo.Arguments =
        "-list_devices true -f dshow -i dummy";

    p.StartInfo.UseShellExecute = false;

    p.StartInfo.RedirectStandardError = true;

    p.StartInfo.CreateNoWindow = true;

    p.Start();

    while (!p.StandardError.EndOfStream)
    {
        string? line =
            p.StandardError.ReadLine();

        if (line == null)
            continue;

        if (!line.Contains("Microphone"))
            continue;

        int a = line.IndexOf('"');

        int b = line.LastIndexOf('"');

        if (a < 0 || b <= a)
            continue;

        devices.Add(
            line.Substring(
                a + 1,
                b - a - 1));
    }

    p.WaitForExit();

    return devices;
}

    private static string? Search(string pattern)
    {
        Process p = new();

        p.StartInfo.FileName = Path.Combine(
            AppContext.BaseDirectory,
            "es.exe");

        p.StartInfo.Arguments = pattern;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = true;

        p.Start();

        while (!p.StandardOutput.EndOfStream)
        {
            string? line = p.StandardOutput.ReadLine();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            line = line.Trim();

            if (!File.Exists(line))
                continue;

            if (!line.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                continue;

            return line;
        }

        p.WaitForExit();

        return null;
    }

    private static IEnumerable<string> SearchAll(string pattern)
    {
        Process p = new();

        p.StartInfo.FileName = Path.Combine(
            AppContext.BaseDirectory,
            "es.exe");

        p.StartInfo.Arguments = pattern;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = true;

        p.Start();

        while (!p.StandardOutput.EndOfStream)
        {
            string? line = p.StandardOutput.ReadLine();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            line = line.Trim();

            if (!File.Exists(line))
                continue;

            yield return line;
        }

        p.WaitForExit();
    }
}