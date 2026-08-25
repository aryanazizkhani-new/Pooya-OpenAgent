using System.Text.Json;

namespace OpenAgent.Runtime;

public sealed class AppConfig
{
    public string EverythingPath { get; set; } = "";
}

public static class ConfigManager
{
    private static readonly string ConfigFile =
        Path.Combine(AppContext.BaseDirectory, "settings.json");

    public static AppConfig Load()
    {
        if (!File.Exists(ConfigFile))
            return new AppConfig();

        try
        {
            string json = File.ReadAllText(ConfigFile);

            return JsonSerializer.Deserialize<AppConfig>(json)
                   ?? new AppConfig();
        }
        catch
        {
            return new AppConfig();
        }
    }

    public static void Save(AppConfig config)
    {
        string json = JsonSerializer.Serialize(
            config,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(ConfigFile, json);
    }

    //------------------------------------------------------
    // First Run
    //------------------------------------------------------

    public static string GetEverythingPath()
    {
        AppConfig cfg = Load();

        if (File.Exists(cfg.EverythingPath))
            return cfg.EverythingPath;

        Console.Clear();

        Console.ForegroundColor = ConsoleColor.Yellow;

        Console.WriteLine("First Time Setup");
        Console.WriteLine();

        Console.ResetColor();

        Console.WriteLine("Please enter the full path to es.exe");

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkGray;

        Console.WriteLine("Example:");

        Console.WriteLine(
            @"C:\Program Files\Everything\es.exe");

        Console.ResetColor();

        Console.WriteLine();

        while (true)
        {
            Console.Write("> ");

            string? path = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(path) &&
                File.Exists(path))
            {
                cfg.EverythingPath = path;

                Save(cfg);

                Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine();

                Console.WriteLine("Configuration saved.");

                Console.ResetColor();

                Thread.Sleep(1000);

                return path;
            }

            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("Invalid file.");

            Console.ResetColor();
        }
    }
}