using System.Diagnostics;

namespace OpenAgent.Runtime;

public static class EsSearcher
{
    //--------------------------------------------------
    // Everything Available?
    //--------------------------------------------------

    public static bool IsAvailable()
    {
        return File.Exists(ConfigManager.GetEverythingPath());
    }

    //--------------------------------------------------
    // Find Agents
    //--------------------------------------------------

    public static List<AgentInfo> FindAgents()
    {
        List<AgentInfo> agents = new();

        string es = ConfigManager.GetEverythingPath();

        if (!File.Exists(es))
            return agents;

        Process p = new();

        p.StartInfo.FileName = es;
        p.StartInfo.Arguments = "*.acs";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = true;

        p.Start();

        Dictionary<string, AgentInfo> unique =
            new(StringComparer.OrdinalIgnoreCase);

        while (!p.StandardOutput.EndOfStream)
        {
            string? line = p.StandardOutput.ReadLine();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            line = line.Trim();

            if (!File.Exists(line))
                continue;

            string name =
                Path.GetFileNameWithoutExtension(line);

            if (name.StartsWith("$"))
                continue;

            if (!unique.ContainsKey(name))
            {
                unique.Add(name,
                    new AgentInfo
                    {
                        Name = name,
                        Path = line
                    });
            }
        }

        p.WaitForExit();

        agents = unique.Values
                       .OrderBy(a => a.Name)
                       .ToList();

        return agents;
    }

    //--------------------------------------------------
    // Print Agents
    //--------------------------------------------------

    public static void PrintAgents()
    {
        var agents = FindAgents();

        if (agents.Count == 0)
        {
            Console.WriteLine("No characters found.");
            return;
        }

        Console.WriteLine();

        for (int i = 0; i < agents.Count; i++)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine($"[{i + 1}] {agents[i].Name}");

            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkGray;

            Console.WriteLine($"     {agents[i].Path}");

            Console.ResetColor();

            Console.WriteLine();
        }
    }
}