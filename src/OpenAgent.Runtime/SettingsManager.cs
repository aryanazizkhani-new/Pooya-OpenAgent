using System.Text.Json;

namespace OpenAgent.Runtime;

public static class SettingsManager
{
    private static readonly string FileName =
        Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "settings1.json");

    public static Settings Current { get; private set; }
        = new();

    //--------------------------------------------------
    // Load
    //--------------------------------------------------

    public static void Load()
    {
        if (!File.Exists(FileName))
        {
            Current = new Settings();
            return;
        }

        try
        {
            string json =
                File.ReadAllText(FileName);

            Settings? settings =
                JsonSerializer.Deserialize<Settings>(json);

            Current =
                settings ?? new Settings();
        }
        catch
        {
            Current = new Settings();
        }
    }

    //--------------------------------------------------
    // Save
    //--------------------------------------------------

    public static void Save()
    {
        string json =
            JsonSerializer.Serialize(
                Current,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        File.WriteAllText(
            FileName,
            json);
    }

    //--------------------------------------------------
    // Delete
    //--------------------------------------------------

    public static void Delete()
    {
        if (File.Exists(FileName))
        {
            File.Delete(FileName);
        }

        Current = new Settings();
    }

    //--------------------------------------------------
    // Exists
    //--------------------------------------------------

    public static bool Exists()
    {
        return File.Exists(FileName);
    }
}