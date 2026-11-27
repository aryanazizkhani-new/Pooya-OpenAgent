namespace OpenAgent.Sample;

using System.Text.Json;
using System.Text.Json.Nodes;

public static class ConsoleUI
{
    //--------------------------------------------------
    // Settings
    //--------------------------------------------------

    private static readonly string SettingsFile =
        Path.Combine(
            AppContext.BaseDirectory,
            "settings.json");

    //--------------------------------------------------
    // Header
    //--------------------------------------------------

    public static void DrawHeader(
        string status = "Ready",
        string character = "None",
        string model = "None")
    {
        Console.ForegroundColor = ConsoleColor.Green;

        Console.WriteLine("""
   ____                  ___                     __
  / __ \____  ___  ____ /   | ____ ____  ____  / /_
 / / / / __ \/ _ \/ __ / /| |/ __ `/ _ \/ __ \/ __/
/ /_/ / /_/ /  __/ / / / ___ / /_/ /  __/ / / / /_
\____/ .___/\___/_/ /_/_/  |_\__, /\___/_/ /_/\__/
    /_/                     /____/

                OpenAgent v1.0
""");

        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.DarkGray;

        Console.WriteLine(
            "==============================================================");

        Console.WriteLine(
            $" Status : {status,-20} Character : {character}");

        Console.WriteLine(
            $" Model  : {model}");

        Console.WriteLine(
            "==============================================================");

        Console.ResetColor();

        Console.WriteLine();
    }

    //--------------------------------------------------
    // Main Menu
    //--------------------------------------------------

    public static void Draw(
        string status = "Ready",
        string character = "None",
        string model = "None")
    {
        Console.Clear();

        DrawHeader(status, character, model);

        Console.ForegroundColor = ConsoleColor.Cyan;

        Console.WriteLine(" MAIN MENU");

        Console.ResetColor();

        Console.WriteLine();

        Console.WriteLine(" [1] Start Assistant");
        Console.WriteLine(" [2] AI Providers");
        Console.WriteLine(" [3] Help");
        Console.WriteLine(" [4] About");
        Console.WriteLine(" [0] Exit");

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkGray;

        Console.WriteLine(
            "--------------------------------------------------------------");

        Console.ResetColor();

        Console.Write("Select > ");
    }

    //--------------------------------------------------
    // AI Providers
    //--------------------------------------------------

    public static void ShowAIProviders(
        string ollamaStatus = "Available",
        string openRouterStatus = "Not configured")
    {
        Console.Clear();

        string savedApiKey =
            GetOpenRouterApiKey();

        if (!string.IsNullOrWhiteSpace(savedApiKey))
        {
            openRouterStatus = "Configured";
        }

        DrawHeader(
            "Ready",
            "None",
            GetOpenRouterModel());

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(" AI PROVIDERS");
        Console.ResetColor();

        Console.WriteLine();

        //--------------------------------------------------
        // Ollama
        //--------------------------------------------------

        Console.WriteLine(" [1] Ollama");

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            $"     Status : {ollamaStatus}");
        Console.ResetColor();

        Console.WriteLine();

        //--------------------------------------------------
        // OpenRouter
        //--------------------------------------------------

        Console.WriteLine(" [2] OpenRouter");

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            $"     Status : {openRouterStatus}");
        Console.ResetColor();

        Console.WriteLine();

        //--------------------------------------------------
        // Configure AI Providers
        //--------------------------------------------------

        Console.WriteLine(" [3] Configure AI Providers");

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            "     Configure provider settings and credentials.");
        Console.ResetColor();

        Console.WriteLine();

        //--------------------------------------------------
        // Footer
        //--------------------------------------------------

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            "--------------------------------------------------------------");
        Console.ResetColor();

        Console.WriteLine(" [0] Back");

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            "==============================================================");
        Console.ResetColor();

        Console.Write("Select > ");
    }

    //--------------------------------------------------
    // AI Provider Configuration
    //--------------------------------------------------

    public static void ShowAIProviderConfiguration()
    {
        Console.Clear();

        DrawHeader(
            "Configuration",
            "None",
            GetOpenRouterModel());

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(" AI PROVIDER CONFIGURATION");
        Console.ResetColor();

        Console.WriteLine();

        Console.WriteLine(
            "Configure the AI providers used by OpenAgent.");

        Console.WriteLine();

        //--------------------------------------------------
        // OpenRouter
        //--------------------------------------------------

        Console.WriteLine(" [1] OpenRouter Configuration");

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            "     Configure API key and model settings.");
        Console.ResetColor();

        Console.WriteLine();

        //--------------------------------------------------
        // Ollama
        //--------------------------------------------------

        Console.WriteLine(" [2] Ollama Configuration");

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            "     Configure local Ollama settings.");
        Console.ResetColor();

        Console.WriteLine();

        //--------------------------------------------------
        // Footer
        //--------------------------------------------------

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            "--------------------------------------------------------------");
        Console.ResetColor();

        Console.WriteLine(" [0] Back");

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            "==============================================================");
        Console.ResetColor();

        Console.Write("Select > ");
    }

    //--------------------------------------------------
    // OpenRouter
    //--------------------------------------------------

    public static void ShowOpenRouter(
        bool apiKeyConfigured = false,
        string model = "None",
        string status = "Not configured")
    {
        string savedApiKey =
            GetOpenRouterApiKey();

        apiKeyConfigured =
            !string.IsNullOrWhiteSpace(savedApiKey);

        string savedModel =
            GetOpenRouterModel();

        if (string.IsNullOrWhiteSpace(model) ||
            model == "None")
        {
            model = savedModel;
        }

        if (apiKeyConfigured &&
            status == "Not configured")
        {
            status = "Configured";
        }

        Console.Clear();

        DrawHeader(
            status,
            "None",
            model);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(" OPENROUTER");
        Console.ResetColor();

        Console.WriteLine();

        Console.WriteLine(
            $" API Key : {MaskOpenRouterApiKey(savedApiKey)}");

        Console.WriteLine(
            $" Model   : {model}");

        Console.WriteLine(
            $" Status  : {status}");

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            "--------------------------------------------------------------");
        Console.ResetColor();

        Console.WriteLine(" [1] Set API Key");
        Console.WriteLine(" [2] Select Model");
        Console.WriteLine(" [3] Test Connection");
        Console.WriteLine(" [4] Clear API Key");
        Console.WriteLine(" [0] Back");

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            "==============================================================");
        Console.ResetColor();

        Console.Write("Select > ");
    }

    //--------------------------------------------------
    // OpenRouter API Key
    //--------------------------------------------------

    public static string ReadOpenRouterApiKey()
    {
        string currentApiKey =
            GetOpenRouterApiKey();

        Console.Clear();

        DrawHeader(
            "Waiting for API Key",
            "None",
            "OpenRouter");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(" OPENROUTER API KEY");
        Console.ResetColor();

        Console.WriteLine();

        //--------------------------------------------------
        // Existing key
        //--------------------------------------------------

        if (!string.IsNullOrWhiteSpace(currentApiKey))
        {
            Console.WriteLine(
                "Current API Key :");

            Console.ForegroundColor =
                ConsoleColor.Green;

            Console.WriteLine(
                MaskOpenRouterApiKey(currentApiKey));

            Console.ResetColor();

            Console.WriteLine();
        }
        else
        {
            Console.ForegroundColor =
                ConsoleColor.Yellow;

            Console.WriteLine(
                "No API key is currently configured.");

            Console.ResetColor();

            Console.WriteLine();
        }

        //--------------------------------------------------
        // Instructions
        //--------------------------------------------------

        Console.WriteLine(
            "Enter your OpenRouter API key.");

        Console.WriteLine();

        Console.ForegroundColor =
            ConsoleColor.DarkGray;

        Console.WriteLine(
            "The key will not be displayed while typing.");

        Console.WriteLine(
            "Press Enter without typing to keep the current key.");

        Console.ResetColor();

        Console.WriteLine();

        Console.Write("API Key > ");

        string input =
            ReadHiddenInput();

        if (string.IsNullOrWhiteSpace(input))
        {
            return currentApiKey;
        }

        return input.Trim();
    }

    //--------------------------------------------------
    // Hidden Input
    //--------------------------------------------------

    private static string ReadHiddenInput()
    {
        string input = "";

        while (true)
        {
            ConsoleKeyInfo keyInfo =
                Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }

            if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (input.Length > 0)
                {
                    input = input[..^1];

                    Console.Write("\b \b");
                }

                continue;
            }

            if (char.IsControl(keyInfo.KeyChar))
                continue;

            input += keyInfo.KeyChar;

            Console.Write("*");
        }

        return input;
    }

    //--------------------------------------------------
    // OpenRouter Model Selection
    //--------------------------------------------------

    public static string SelectOpenRouterModel(
        List<string> models,
        string currentModel = "None")
    {
        Console.Clear();

        DrawHeader(
            "Ready",
            "None",
            currentModel);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(" OPENROUTER MODELS");
        Console.ResetColor();

        Console.WriteLine();

        Console.WriteLine(
            $" Current Model : {currentModel}");

        Console.WriteLine();

        if (models == null || models.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine(
                " No OpenRouter models are currently available.");

            Console.ResetColor();

            Console.WriteLine();

            Wait();

            return currentModel;
        }

        Console.WriteLine(
            $" Available Models : {models.Count}");

        Console.WriteLine();

        for (int i = 0; i < models.Count; i++)
        {
            Console.WriteLine(
                $" [{i + 1}] {models[i]}");
        }

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            "--------------------------------------------------------------");
        Console.ResetColor();

        Console.WriteLine(" [0] Back");

        Console.WriteLine();

        Console.Write("Select > ");

        string input =
            Console.ReadLine() ?? "";

        if (input == "0")
            return currentModel;

        if (int.TryParse(
            input,
            out int selected))
        {
            if (selected >= 1 &&
                selected <= models.Count)
            {
                return models[selected - 1];
            }
        }

        Error(
            "Invalid model selection.");

        Wait();

        return currentModel;
    }

    //--------------------------------------------------
    // OpenRouter Connection
    //--------------------------------------------------

    public static void ShowOpenRouterConnection(
        string status)
    {
        Console.Clear();

        DrawHeader(
            status,
            "None",
            GetOpenRouterModel());

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(" OPENROUTER CONNECTION");
        Console.ResetColor();

        Console.WriteLine();

        Console.WriteLine(
            $" Status : {status}");

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            "--------------------------------------------------------------");
        Console.ResetColor();

        Wait();
    }

    //--------------------------------------------------
    // AI Models - Ollama
    //--------------------------------------------------

    public static void ShowModels(
        List<string> models,
        string currentModel = "None")
    {
        Console.Clear();

        DrawHeader(
            "Ready",
            "None",
            currentModel);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(" AI MODELS");
        Console.ResetColor();

        Console.WriteLine();

        Console.WriteLine(
            $" Current Model : {currentModel}");

        Console.WriteLine();

        Console.WriteLine(
            $" Installed Models : {models.Count}");

        Console.WriteLine();

        for (int i = 0; i < models.Count; i++)
        {
            Console.WriteLine(
                $" [{i + 1}] {models[i]}");
        }

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            "--------------------------------------------------------------");
        Console.ResetColor();

        Console.WriteLine(" [D] Download Model");
        Console.WriteLine(" [R] Refresh");
        Console.WriteLine(" [0] Back");

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            "==============================================================");
        Console.ResetColor();

        Console.Write("Select > ");
    }

    //--------------------------------------------------
    // Load Character
    //--------------------------------------------------

    public static void ShowLoadCharacter(
        string model = "None")
    {
        Console.Clear();

        DrawHeader(
            "Ready",
            "None",
            model);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(" LOAD CHARACTER");
        Console.ResetColor();

        Console.WriteLine();

        Warning(
            "It is recommended to run ONE AI Model and ONE Agent.");

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            "--------------------------------------------------------------");
        Console.ResetColor();
    }

    //--------------------------------------------------
    // Help
    //--------------------------------------------------

    public static void ShowHelp()
    {
        Console.Clear();

        DrawHeader();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("HELP");
        Console.WriteLine(
            "==============================================================");
        Console.ResetColor();

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;

        Console.WriteLine(
            "Note: Take messages who have short answer because it can");
        Console.WriteLine(
            "take very space and come out from your desktop!");
        Console.WriteLine(
            "We fix it in next updates!");

        Console.ResetColor();

        Console.WriteLine();

        //--------------------------------------------------
        // Main Menu
        //--------------------------------------------------

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("MAIN MENU");
        Console.ResetColor();

        Console.WriteLine();

        Console.WriteLine("1  Start Assistant");
        Console.WriteLine(
            "   Start an OpenAgent conversation.");
        Console.WriteLine();

        Console.WriteLine("2  AI Providers");
        Console.WriteLine(
            "   Manage the AI providers used by OpenAgent.");
        Console.WriteLine();

        Console.WriteLine("3  Help");
        Console.WriteLine(
            "   Display this help page.");
        Console.WriteLine();

        Console.WriteLine("4  About");
        Console.WriteLine(
            "   Show project information.");
        Console.WriteLine();

        Console.WriteLine("0  Exit");
        Console.WriteLine(
            "   Close OpenAgent.");

        Console.WriteLine();

        //--------------------------------------------------
        // AI Providers
        //--------------------------------------------------

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("AI PROVIDERS");
        Console.ResetColor();

        Console.WriteLine();

        Console.WriteLine(
            "AI Providers allow OpenAgent to use different");
        Console.WriteLine(
            "AI backends for conversations.");

        Console.WriteLine();

        Console.WriteLine("Ollama");
        Console.WriteLine(
            "   Run AI models locally on your computer.");
        Console.WriteLine(
            "   Internet connection is not required after");
        Console.WriteLine(
            "   the required model has been downloaded.");

        Console.WriteLine();

        Console.WriteLine("OpenRouter");
        Console.WriteLine(
            "   Connect OpenAgent to AI models through");
        Console.WriteLine(
            "   the OpenRouter API.");

        Console.WriteLine();

        //--------------------------------------------------
        // OpenRouter
        //--------------------------------------------------

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("OPENROUTER");
        Console.ResetColor();

        Console.WriteLine();

        Console.WriteLine(
            "OpenRouter provides access to many different");
        Console.WriteLine(
            "AI models through one API.");

        Console.WriteLine();

        Console.WriteLine("API Key");
        Console.WriteLine(
            "   OpenRouter requires an API key to send");
        Console.WriteLine(
            "   requests to its API.");

        Console.WriteLine();

        Console.WriteLine(
            "   OpenAgent will ask you for your API key");
        Console.WriteLine(
            "   when you configure OpenRouter.");

        Console.WriteLine();

        Console.WriteLine("Models");
        Console.WriteLine(
            "   OpenRouter provides a list of available");
        Console.WriteLine(
            "   AI models that can be selected by the user.");

        Console.WriteLine();

        Console.WriteLine("Free and Paid Models");
        Console.WriteLine(
            "   Some models may be available with free");
        Console.WriteLine(
            "   usage, while other models may require");
        Console.WriteLine(
            "   payment or have usage costs.");

        Console.WriteLine();

        Console.WriteLine("Security");
        Console.WriteLine(
            "   Your API key is sensitive information.");
        Console.WriteLine(
            "   Do not share your API key with other people.");

        Console.WriteLine();

        //--------------------------------------------------
        // OpenRouter Workflow
        //--------------------------------------------------

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("OPENROUTER WORKFLOW");
        Console.ResetColor();

        Console.WriteLine();

        Console.WriteLine(
            "1. Open AI Providers.");
        Console.WriteLine(
            "2. Select OpenRouter.");
        Console.WriteLine(
            "3. Configure your API key.");
        Console.WriteLine(
            "4. Select an available AI model.");
        Console.WriteLine(
            "5. OpenAgent sends requests using the selected model.");
        Console.WriteLine(
            "6. The AI response is returned to OpenAgent.");

        Console.WriteLine();

        //--------------------------------------------------
        // Configuration
        //--------------------------------------------------

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("AI PROVIDER CONFIGURATION");
        Console.ResetColor();

        Console.WriteLine();

        Console.WriteLine(
            "Use Configure AI Providers to manage");
        Console.WriteLine(
            "provider settings and credentials.");

        Console.WriteLine();

        Console.WriteLine(
            "OpenRouter configuration can include");
        Console.WriteLine(
            "your API key and selected model.");

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkGray;

        Console.WriteLine(
            "Provider configuration is stored locally by OpenAgent.");

        Console.WriteLine(
            "API keys should never be shared with other people.");

        Console.ResetColor();

        Console.WriteLine();

        //--------------------------------------------------
        // Footer
        //--------------------------------------------------

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            "==============================================================");
        Console.ResetColor();

        Wait();
    }

    //--------------------------------------------------
    // About
    //--------------------------------------------------

    public static void ShowAbout()
    {
        Console.Clear();

        DrawHeader();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("ABOUT OPENAGENT");
        Console.WriteLine(
            "==============================================================");
        Console.ResetColor();

        Console.WriteLine();

        Console.WriteLine("Project");
        Console.WriteLine("   OpenAgent");

        Console.WriteLine();

        Console.WriteLine("Version");
        Console.WriteLine("   1.0");

        Console.WriteLine();

        Console.WriteLine("Framework");
        Console.WriteLine("   .NET 10");

        Console.WriteLine();

        Console.WriteLine("Features");
        Console.WriteLine("   • Microsoft Agent");
        Console.WriteLine("   • Double Agent");
        Console.WriteLine("   • Ollama AI");
        Console.WriteLine("   • OpenRouter AI");
        Console.WriteLine("   • Everything Search");
        Console.WriteLine("   • Offline AI");

        Console.WriteLine();

        Console.WriteLine("Developer");
        Console.WriteLine("   Aryan Azizkhani");

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            "==============================================================");
        Console.ResetColor();

        Wait();
    }

    //--------------------------------------------------
    // Settings - Load
    //--------------------------------------------------

    private static JsonObject LoadSettings()
    {
        try
        {
            if (!File.Exists(SettingsFile))
            {
                return new JsonObject();
            }

            string json =
                File.ReadAllText(SettingsFile);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new JsonObject();
            }

            JsonNode? node =
                JsonNode.Parse(json);

            if (node is JsonObject settings)
            {
                return settings;
            }

            return new JsonObject();
        }
        catch
        {
            return new JsonObject();
        }
    }

    //--------------------------------------------------
    // Settings - Save
    //--------------------------------------------------

    private static bool SaveSettings(
        JsonObject settings)
    {
        try
        {
            string? directory =
                Path.GetDirectoryName(SettingsFile);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            JsonSerializerOptions options =
                new()
                {
                    WriteIndented = true
                };

            string json =
                settings.ToJsonString(options);

            File.WriteAllText(
                SettingsFile,
                json);

            return true;
        }
        catch (Exception ex)
        {
            Error(
                "Cannot save settings: " +
                ex.Message);

            return false;
        }
    }

    //--------------------------------------------------
    // OpenRouter Settings Object
    //--------------------------------------------------

    private static JsonObject GetOpenRouterSettings(
        JsonObject settings)
    {
        JsonNode? node =
            settings["OpenRouter"];

        if (node is JsonObject openRouter)
        {
            return openRouter;
        }

        JsonObject newOpenRouter =
            new();

        settings["OpenRouter"] =
            newOpenRouter;

        return newOpenRouter;
    }

    //--------------------------------------------------
    // Get OpenRouter API Key
    //--------------------------------------------------

    public static string GetOpenRouterApiKey()
    {
        JsonObject settings =
            LoadSettings();

        JsonObject openRouter =
            GetOpenRouterSettings(settings);

        return
            openRouter["ApiKey"]?.GetValue<string>()
            ?? "";
    }

    //--------------------------------------------------
    // Get OpenRouter Model
    //--------------------------------------------------

    public static string GetOpenRouterModel()
    {
        JsonObject settings =
            LoadSettings();

        JsonObject openRouter =
            GetOpenRouterSettings(settings);

        return
            openRouter["Model"]?.GetValue<string>()
            ?? "None";
    }

    //--------------------------------------------------
    // Set OpenRouter API Key
    //--------------------------------------------------

    public static bool SetOpenRouterApiKey(
        string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return false;
        }

        JsonObject settings =
            LoadSettings();

        JsonObject openRouter =
            GetOpenRouterSettings(settings);

        openRouter["ApiKey"] =
            apiKey.Trim();

        return SaveSettings(settings);
    }

    //--------------------------------------------------
    // Clear OpenRouter API Key
    //--------------------------------------------------

    public static bool ClearOpenRouterApiKey()
    {
        JsonObject settings =
            LoadSettings();

        JsonObject openRouter =
            GetOpenRouterSettings(settings);

        openRouter["ApiKey"] = "";

        return SaveSettings(settings);
    }

    //--------------------------------------------------
    // Set OpenRouter Model
    //--------------------------------------------------

    public static bool SetOpenRouterModel(
        string model)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            return false;
        }

        JsonObject settings =
            LoadSettings();

        JsonObject openRouter =
            GetOpenRouterSettings(settings);

        openRouter["Model"] =
            model.Trim();

        return SaveSettings(settings);
    }

    //--------------------------------------------------
    // Mask OpenRouter API Key
    //--------------------------------------------------

    public static string MaskOpenRouterApiKey(
        string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return "Not configured";
        }

        apiKey =
            apiKey.Trim();

        //--------------------------------------------------
        // Very short keys
        //--------------------------------------------------

        if (apiKey.Length <= 5)
        {
            return new string(
                '*',
                apiKey.Length);
        }

        //--------------------------------------------------
        // Exactly 5 characters in the middle
        //--------------------------------------------------

        int middleLength = 5;

        int remaining =
            apiKey.Length - middleLength;

        int leftLength =
            remaining / 2;

        int rightLength =
            remaining - leftLength;

        string left =
            apiKey[..leftLength];

        string right =
            apiKey[^rightLength..];

        return
            left +
            "*****" +
            right;
    }

    //--------------------------------------------------
    // Messages
    //--------------------------------------------------

    public static void Success(string text)
    {
        Console.ForegroundColor =
            ConsoleColor.Green;

        Console.WriteLine(
            "[OK] " + text);

        Console.ResetColor();
    }

    public static void Error(string text)
    {
        Console.ForegroundColor =
            ConsoleColor.Red;

        Console.WriteLine(
            "[ERROR] " + text);

        Console.ResetColor();
    }

    public static void Warning(string text)
    {
        Console.ForegroundColor =
            ConsoleColor.Yellow;

        Console.WriteLine(
            "[WARNING] " + text);

        Console.ResetColor();
    }

    //--------------------------------------------------
    // Wait
    //--------------------------------------------------

    public static void Wait()
    {
        Console.WriteLine();

        Console.ForegroundColor =
            ConsoleColor.DarkGray;

        Console.Write(
            "Press any key to continue...");

        Console.ResetColor();

        Console.ReadKey(true);
    }
}