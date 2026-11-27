using OpenAgent.AI;
using OpenAgent.Runtime;

namespace OpenAgent.Sample;

public enum ChatMode
{
    Keyboard,
    Voice,
    Auto
}

public sealed class AgentManager : IDisposable
{
    public event Action<CrashInfo>? OnCharacterDead;
    public event Action? OnAgentCrash;
    //--------------------------------------------------
    // Fields
    //--------------------------------------------------
    private int screenWidth;
private int screenHeight;
private int currentX;
private int currentY;

private readonly SemaphoreSlim moveLock = new(1,1);
private CancellationTokenSource? movementToken;

private bool isMoving = false;

    private string currentAgentPath = "";

private string currentModel = "";

private string currentFFmpeg = "";

private string currentWhisperExe = "";

private string currentWhisperModel = "";

    private readonly OpenAgentAssistant assistant;

    private AgentLoader? agent;
    private string microphoneName = "";
private CancellationTokenSource? watchdogToken;
private bool recovering = false;

    //--------------------------------------------------
    // Constructor
    //--------------------------------------------------

    public AgentManager()
    {
        assistant = new OpenAgentAssistant();

        assistant.OnVoiceText +=
            Assistant_OnVoiceText;
    }
 

    //--------------------------------------------------
    // Chat Mode
    //--------------------------------------------------

    public ChatMode Mode
    {
        get;
        set;
    }
    = ChatMode.Keyboard;

    //--------------------------------------------------
    // Properties
    //--------------------------------------------------

    public bool HasAgent
    {
        get
        {
            return agent != null;
        }
    }

    public AgentLoader? CurrentAgent
    {
        get
        {
            return agent;
        }
    }

    //--------------------------------------------------
    // Voice Event
    //--------------------------------------------------

    public event Action<string>? OnVoiceText
    {
        add
        {
            assistant.OnVoiceText += value;
        }

        remove
        {
            assistant.OnVoiceText -= value;
        }
    }
    public void SetScreenResolution(
    int width,
    int height)
{
    screenWidth = width;
    screenHeight = height;



    Console.ForegroundColor = ConsoleColor.Green;

    Console.WriteLine(
        $"Screen : {width}x{height}");



    Console.ResetColor();
}


    //--------------------------------------------------
    // Voice
    //--------------------------------------------------

public void ConfigureVoice(
    string ffmpegExe,
    string whisperExe,
    string whisperModel,
    string microphone)
{
    currentFFmpeg = ffmpegExe;

    currentWhisperExe = whisperExe;

    currentWhisperModel = whisperModel;

    microphoneName = microphone;

    assistant.ConfigureVoice(
        ffmpegExe,
        whisperExe,
        whisperModel,
        microphone);
}

 
    public void StartVoice()
    {
        assistant.StartVoice();
    }

//--------------------------------------------------
// Stop Voice
//--------------------------------------------------

public async Task<string?> StopVoiceAsync()
{
    try
    {
        string? text =
            await assistant.StopVoiceAsync();

        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return text.Trim();
    }
    catch (Exception ex)
    {
        Console.WriteLine();
        Console.WriteLine("===== VOICE STOP ERROR =====");
        Console.WriteLine(ex.ToString());
        Console.WriteLine("============================");

        return null;
    }
}
    //--------------------------------------------------
    // Voice Callback
    //--------------------------------------------------

    private async void Assistant_OnVoiceText(
        string text)
    {
        if (Mode == ChatMode.Keyboard)
            return;

        if (string.IsNullOrWhiteSpace(text))
            return;

        try
        {
string answer =
    await assistant.AskSelectedProviderAsync(text);

            if (!string.IsNullOrWhiteSpace(answer))
            {
                await assistant.SpeakAsync(answer);
            }
        }
        catch
        {
        }
    }

    //--------------------------------------------------
    // Character
    //--------------------------------------------------
        public bool LoadCharacter(
        string acsFile)
    {
        currentAgentPath = acsFile;
        try
        {
            AgentLoader loader =
                new();

            loader.Load(acsFile);

            AttachAgent(loader);

            return true;
        }
        catch
        {
            return false;
        }
    }
public bool RecoverCharacter()
{
    if (string.IsNullOrWhiteSpace(currentAgentPath))
        return false;

    try
    {
        Console.WriteLine("[RECOVERY] Closing old agent...");

        CloseCharacter();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Console.WriteLine("[RECOVERY] Waiting 10 sec...");

        Thread.Sleep(10000);

        Console.WriteLine("[RECOVERY] Creating NEW loader...");

        AgentLoader loader = new();

        Console.WriteLine("[RECOVERY] Loading character...");

        loader.Load(currentAgentPath);

        Console.WriteLine("[RECOVERY] Attaching...");

        AttachAgent(loader, false);

        Console.WriteLine("[RECOVERY] Waiting 5 sec before first Speak...");

        Thread.Sleep(5000);

        Console.WriteLine(
            $"[RECOVERY] Character = {(loader.Character != null)}");

        try
        {
            loader.Speak("Recovery completed.");
            Console.WriteLine("[RECOVERY] Speak OK");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[RECOVERY] Speak FAILED");
            Console.WriteLine(ex);
        }

        Console.WriteLine("[RECOVERY] Finished.");

        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine();
        Console.WriteLine("========== RECOVERY ERROR ==========");
        Console.WriteLine(ex);
        Console.WriteLine("===================================");

        return false;
    }
}
public void AttachAgent(
    AgentLoader loader,
    bool startWatchdog = true)
{
    agent = loader;

    agent.OnCharacterDead += Agent_OnCharacterDead;

    assistant.AttachAgent(loader);

    if (startWatchdog)
        StartWatchdog();
}


public void CloseCharacter()
{
    if (agent == null)
        return;

    try
    {
        Console.WriteLine("[RECOVERY] Stopping old character...");

        agent.Stop();

        Thread.Sleep(500);

        Console.WriteLine("[RECOVERY] Hiding old character...");

        agent.Hide();

        Thread.Sleep(500);

        agent.Dispose();
    }
    catch (Exception ex)
    {
        Console.WriteLine("[RECOVERY CLOSE ERROR]");
        Console.WriteLine(ex.Message);
    }

    RemoveAgent();
}
public void ForceShow()
{
    if (agent == null)
        return;

    agent.ForceShow();
}


public void RemoveAgent()
{
    watchdogToken?.Cancel();

    assistant.DetachAgent();

    if (agent != null)
        agent.OnCharacterDead -= Agent_OnCharacterDead;

    agent = null;
}

    //--------------------------------------------------
    // Askacync
    //--------------------------------------------------

public async Task<string> AskAsync(string message)
{
    return await assistant.AskSelectedProviderAsync(message);
}


    //--------------------------------------------------
    // Streaming
    //--------------------------------------------------

    public async Task StreamAsync(
        string message,
        Action<string> onToken)
    {
        await assistant.StreamAsync(
            message,
            onToken);
    }

    //--------------------------------------------------
    // Speak
    //--------------------------------------------------

    public async Task SpeakAsync(
        string text)
    {
        await assistant.SpeakAsync(text);
    }

    public async Task SpeakStreamingAsync(
        string text)
    {
        await assistant.SpeakStreamingAsync(text);
    }

public async Task Speak(string text)
{
    if (agent == null)
        return;

    await agent.SmartSpeak(text);
}

    //--------------------------------------------------
    // Stop AI
    //--------------------------------------------------

    public void StopGeneration()
    {
        assistant.Stop();
    }

    //--------------------------------------------------
    // Models
    //--------------------------------------------------
        public async Task<bool> LoadModelAsync(
        string model)
    {
        return await assistant
            .LoadModelAsync(model);
    }

    public async Task<bool> DownloadModelAsync(
        string model,
        Action<string>? progress = null)
    {
        return await assistant
            .DownloadModelAsync(
                model,
                progress);
    }

    public async Task<List<string>>
        GetModelsAsync()
    {
        return await assistant
            .GetInstalledModelsAsync();
    }

    //--------------------------------------------------
    // Conversation
    //--------------------------------------------------

    public void ClearConversation()
    {
        assistant.ClearChat();
    }

    public string ExportConversation()
    {
        return assistant.ExportChat();
    }
//--------------------------------------------------
// AI Provider Configuration
//--------------------------------------------------

public void SetAIProvider(AIProvider provider)
{
    assistant.SetProvider(provider);
}

public void ConfigureOpenRouter(
    string apiKey,
    string model)
{
    assistant.ConfigureOpenRouter(
        apiKey,
        model);

    assistant.SetProvider(
        AIProvider.OpenRouter);
}
    //--------------------------------------------------
    // Console Commands
    //--------------------------------------------------

public void HandleCommand(string? command)
{
    if (string.IsNullOrWhiteSpace(command))
        return;

    command = command.Trim();

    //--------------------------------------------------
    // /play <animation>
    //--------------------------------------------------

    if (command.StartsWith(
        "/play ",
        StringComparison.OrdinalIgnoreCase))
    {
        if (agent == null)
            return;

        string animation =
            command.Substring(6).Trim();

        if (string.IsNullOrWhiteSpace(animation))
        {
            ConsoleUI.Warning(
                "Usage: /play <animation>");

            ConsoleUI.Wait();

            return;
        }

        try
        {
            agent.Play(animation);
        }
        catch (Exception ex)
        {
            ConsoleUI.Error(
                "Cannot play animation: " +
                ex.Message);

            ConsoleUI.Wait();
        }

        return;
    }

    //--------------------------------------------------
    // Move Commands
    //--------------------------------------------------

    if (command.StartsWith(
        "/move",
        StringComparison.OrdinalIgnoreCase))
    {
        string data =
            command.Length > 5
                ? command.Substring(5).Trim()
                : "";

        string[] args =
            data.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

        var pos =
            MoveCommand.Parse(args);

        if (pos != null)
        {
            _ = Move(
                pos.Value.x,
                pos.Value.y);
        }
        else
        {
            Console.WriteLine(
                "[MOVE] Invalid command.");
        }

        return;
    }

    //--------------------------------------------------
    // Main Menu
    //--------------------------------------------------

    switch (command)
    {
        //--------------------------------------------------
        // 1 - Load AI Model
        //--------------------------------------------------

        case "1":
        {
            Console.Clear();

            Console.ForegroundColor =
                ConsoleColor.Cyan;

            Console.WriteLine(
                "LOAD AI MODEL");

            Console.WriteLine(
                "======================================");

            Console.ResetColor();

            List<string> models =
                GetModelsAsync()
                    .GetAwaiter()
                    .GetResult();

            if (models.Count == 0)
            {
                ConsoleUI.Error(
                    "No Ollama models found.");

                ConsoleUI.Wait();

                return;
            }

            Console.WriteLine();

            for (int i = 0;
                 i < models.Count;
                 i++)
            {
                Console.WriteLine(
                    $"[{i + 1}] {models[i]}");
            }

            Console.WriteLine();

            Console.Write(
                "Select Model : ");

            if (!int.TryParse(
                Console.ReadLine(),
                out int index))
            {
                ConsoleUI.Error(
                    "Invalid selection.");

                ConsoleUI.Wait();

                return;
            }

            index--;

            if (index < 0 ||
                index >= models.Count)
            {
                ConsoleUI.Error(
                    "Invalid selection.");

                ConsoleUI.Wait();

                return;
            }

            Console.WriteLine();
            Console.WriteLine(
                "Loading model...");
            Console.WriteLine();

            bool ok =
                LoadModelAsync(
                    models[index])
                .GetAwaiter()
                .GetResult();

            if (ok)
            {
                currentModel =
                    models[index];

                ConsoleUI.Success(
                    $"Current Model : {models[index]}");
            }
            else
            {
                ConsoleUI.Error(
                    "Cannot load model.");
            }

            ConsoleUI.Wait();

            return;
        }

        //--------------------------------------------------
        // 2 - AI Providers
        //--------------------------------------------------

        case "2":
        {
        AIProvidersBack:

            Console.Clear();

            ConsoleUI.DrawHeader(
                "AI Providers",
                assistant.ProviderName,
                currentModel);

            Console.WriteLine();

            Console.ForegroundColor =
                ConsoleColor.Cyan;

            Console.WriteLine(
                " AI PROVIDERS");

            Console.ResetColor();

            Console.WriteLine();

            Console.WriteLine(
                $" Current Provider : {assistant.ProviderName}");

            Console.WriteLine();

            Console.WriteLine(
                " [1] Ollama");

            Console.WriteLine(
                " [2] OpenRouter");

            Console.WriteLine(
                " [3] Configure AI Providers");

            Console.WriteLine(
                " [0] Back");

            Console.WriteLine();

            Console.Write(
                "Select > ");

            string providerChoice =
                Console.ReadLine() ?? "";

            providerChoice =
                providerChoice.Trim();

            switch (providerChoice)
            {
                //--------------------------------------------------
                // Ollama
                //--------------------------------------------------

                case "1":
                {
                    assistant.SetProvider(
                        AIProvider.Ollama);

                    Console.Clear();

                    ConsoleUI.DrawHeader(
                        "Ollama",
                        "Ollama",
                        currentModel);

                    Console.WriteLine();

                    Console.ForegroundColor =
                        ConsoleColor.Cyan;

                    Console.WriteLine(
                        " OLLAMA");

                    Console.ResetColor();

                    Console.WriteLine();

                    ConsoleUI.Success(
                        "Ollama selected as the active AI provider.");

                    Console.WriteLine();

                    Console.WriteLine(
                        "Current Provider : Ollama");

                    ConsoleUI.Wait();

                    goto AIProvidersBack;
                }

                //--------------------------------------------------
                // OpenRouter
                //--------------------------------------------------

                case "2":
                {
                    string apiKey =
                        ConsoleUI.GetOpenRouterApiKey();

                    string model =
                        ConsoleUI.GetOpenRouterModel();

                    assistant.ConfigureOpenRouter(
                        apiKey,
                        model);

                    assistant.SetProvider(
                        AIProvider.OpenRouter);

                OpenRouterBack:

                    Console.Clear();

                    apiKey =
                        ConsoleUI.GetOpenRouterApiKey();

                    model =
                        ConsoleUI.GetOpenRouterModel();

                    assistant.ConfigureOpenRouter(
                        apiKey,
                        model);

                    ConsoleUI.ShowOpenRouter(
                        !string.IsNullOrWhiteSpace(apiKey),
                        string.IsNullOrWhiteSpace(model)
                            ? "None"
                            : model,
                        string.IsNullOrWhiteSpace(apiKey)
                            ? "Not configured"
                            : "Configured");

                    Console.WriteLine();

                    Console.Write(
                        "Select > ");

                    string openRouterChoice =
                        Console.ReadLine() ?? "";

                    openRouterChoice =
                        openRouterChoice.Trim();

                    switch (openRouterChoice)
                    {
                        //--------------------------------------------------
                        // 1 - Set API Key
                        //--------------------------------------------------

                        case "1":
                        {
                            string newApiKey =
                                ConsoleUI.ReadOpenRouterApiKey();

                            if (string.IsNullOrWhiteSpace(
                                newApiKey))
                            {
                                ConsoleUI.Error(
                                    "API key cannot be empty.");

                                ConsoleUI.Wait();

                                goto OpenRouterBack;
                            }

                            if (ConsoleUI.SetOpenRouterApiKey(
                                newApiKey))
                            {
                                string savedModel =
                                    ConsoleUI.GetOpenRouterModel();

                                assistant.ConfigureOpenRouter(
                                    newApiKey,
                                    savedModel);

                                ConsoleUI.Success(
                                    "OpenRouter API key saved.");

                                Console.WriteLine();

                                Console.WriteLine(
                                    "Current API Key : " +
                                    ConsoleUI.MaskOpenRouterApiKey(
                                        newApiKey));
                            }
                            else
                            {
                                ConsoleUI.Error(
                                    "Cannot save OpenRouter API key.");
                            }

                            ConsoleUI.Wait();

                            goto OpenRouterBack;
                        }

                        //--------------------------------------------------
                        // 2 - Select Model
                        //--------------------------------------------------

                        case "2":
                        {
                            string savedApiKey =
                                ConsoleUI.GetOpenRouterApiKey();

                            if (string.IsNullOrWhiteSpace(
                                savedApiKey))
                            {
                                ConsoleUI.Error(
                                    "Configure an OpenRouter API key first.");

                                ConsoleUI.Wait();

                                goto OpenRouterBack;
                            }

                            try
                            {
                                Console.Clear();

                                ConsoleUI.DrawHeader(
                                    "OpenRouter Models",
                                    "OpenRouter",
                                    currentModel);

                                Console.WriteLine();

                                Console.ForegroundColor =
                                    ConsoleColor.Cyan;

                                Console.WriteLine(
                                    " OPENROUTER MODELS");

                                Console.ResetColor();

                                Console.WriteLine();

                                Console.WriteLine(
                                    "Loading models from OpenRouter...");

                                Console.WriteLine();

                                using var client =
                                    new System.Net.Http.HttpClient();

                                client.Timeout =
                                    TimeSpan.FromSeconds(30);

                                client.DefaultRequestHeaders.Authorization =
                                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                                        "Bearer",
                                        savedApiKey);

                                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                                    "OpenAgent/1.0");

                                System.Net.Http.HttpResponseMessage response =
                                    client.GetAsync(
                                        "https://openrouter.ai/api/v1/models")
                                    .GetAwaiter()
                                    .GetResult();

                                string json =
                                    response.Content
                                        .ReadAsStringAsync()
                                        .GetAwaiter()
                                        .GetResult();

                                if (!response.IsSuccessStatusCode)
                                {
                                    ConsoleUI.Error(
                                        "OpenRouter returned HTTP " +
                                        (int)response.StatusCode);

                                    Console.WriteLine();
                                    Console.WriteLine(json);

                                    ConsoleUI.Wait();

                                    goto OpenRouterBack;
                                }

                                using System.Text.Json.JsonDocument document =
                                    System.Text.Json.JsonDocument.Parse(json);

                                if (!document.RootElement.TryGetProperty(
                                    "data",
                                    out System.Text.Json.JsonElement data))
                                {
                                    ConsoleUI.Error(
                                        "OpenRouter returned an invalid model list.");

                                    ConsoleUI.Wait();

                                    goto OpenRouterBack;
                                }

                                List<string> models =
                                    new();

                                foreach (
                                    System.Text.Json.JsonElement modelElement
                                    in data.EnumerateArray())
                                {
                                    if (!modelElement.TryGetProperty(
                                        "id",
                                        out System.Text.Json.JsonElement idElement))
                                    {
                                        continue;
                                    }

                                    string? modelId =
                                        idElement.GetString();

                                    if (!string.IsNullOrWhiteSpace(modelId))
                                    {
                                        models.Add(modelId);
                                    }
                                }

                                models =
                                    models
                                        .Distinct(
                                            StringComparer.OrdinalIgnoreCase)
                                        .OrderBy(
                                            x => x,
                                            StringComparer.OrdinalIgnoreCase)
                                        .ToList();

                                if (models.Count == 0)
                                {
                                    ConsoleUI.Error(
                                        "No OpenRouter models were returned.");

                                    ConsoleUI.Wait();

                                    goto OpenRouterBack;
                                }

                                //--------------------------------------------------
                                // MODEL CATEGORY MENU
                                //--------------------------------------------------

                                while (true)
                                {
                                    Console.Clear();

                                    ConsoleUI.DrawHeader(
                                        "OpenRouter Models",
                                        "OpenRouter",
                                        currentModel);

                                    Console.WriteLine();

                                    Console.ForegroundColor =
                                        ConsoleColor.Cyan;

                                    Console.WriteLine(
                                        " OPENROUTER MODEL CATEGORIES");

                                    Console.ResetColor();

                                    Console.WriteLine();

                                    Console.WriteLine(
                                        $" Available Models : {models.Count}");

                                    Console.WriteLine();

                                    Console.WriteLine(
                                        " [1] OpenAI Models");

                                    Console.WriteLine(
                                        " [2] Qwen Models");

                                    Console.WriteLine(
                                        " [3] Google Gemini Models");

                                    Console.WriteLine(
                                        " [4] NVIDIA Models");

                                    Console.WriteLine(
                                        " [5] Z.ai / xAI Models");

                                    Console.WriteLine(
                                        " [6] Free Models");

                                    Console.WriteLine(
                                        " [7] Other Models");

                                    Console.WriteLine();

                                    Console.WriteLine(
                                        " [0] Back");

                                    Console.WriteLine();

                                    Console.Write(
                                        "Select > ");

                                    string? categoryInput =
                                        Console.ReadLine();

                                    if (categoryInput == "0")
                                    {
                                        goto OpenRouterBack;
                                    }

                                    string categoryName =
                                        "";

                                    List<string> categoryModels =
                                        new();

                                    switch (categoryInput)
                                    {
                                        case "1":
                                        {
                                            categoryName =
                                                "OPENAI MODELS";

                                            categoryModels =
                                                models
                                                    .Where(
                                                        x => x.StartsWith(
                                                            "openai/",
                                                            StringComparison.OrdinalIgnoreCase))
                                                    .ToList();

                                            break;
                                        }

                                        case "2":
                                        {
                                            categoryName =
                                                "QWEN MODELS";

                                            categoryModels =
                                                models
                                                    .Where(
                                                        x => x.StartsWith(
                                                            "qwen/",
                                                            StringComparison.OrdinalIgnoreCase))
                                                    .ToList();

                                            break;
                                        }

                                        case "3":
                                        {
                                            categoryName =
                                                "GOOGLE GEMINI MODELS";

                                            categoryModels =
                                                models
                                                    .Where(
                                                        x =>
                                                            x.StartsWith(
                                                                "google/",
                                                                StringComparison.OrdinalIgnoreCase)
                                                            &&
                                                            x.Contains(
                                                                "gemini",
                                                                StringComparison.OrdinalIgnoreCase))
                                                    .ToList();

                                            break;
                                        }

                                        case "4":
                                        {
                                            categoryName =
                                                "NVIDIA MODELS";

                                            categoryModels =
                                                models
                                                    .Where(
                                                        x => x.StartsWith(
                                                            "nvidia/",
                                                            StringComparison.OrdinalIgnoreCase))
                                                    .ToList();

                                            break;
                                        }

                                        case "5":
                                        {
                                            categoryName =
                                                "Z.AI / X.AI MODELS";

                                            categoryModels =
                                                models
                                                    .Where(
                                                        x =>
                                                            x.StartsWith(
                                                                "z-ai/",
                                                                StringComparison.OrdinalIgnoreCase)
                                                            ||
                                                            x.StartsWith(
                                                                "x-ai/",
                                                                StringComparison.OrdinalIgnoreCase))
                                                    .ToList();

                                            break;
                                        }

                                        case "6":
                                        {
                                            categoryName =
                                                "FREE MODELS";

                                            categoryModels =
                                                models
                                                    .Where(
                                                        x =>
                                                            x.EndsWith(
                                                                ":free",
                                                                StringComparison.OrdinalIgnoreCase)
                                                            ||
                                                            x.Equals(
                                                                "openrouter/free",
                                                                StringComparison.OrdinalIgnoreCase))
                                                    .ToList();

                                            break;
                                        }

                                        case "7":
                                        {
                                            categoryName =
                                                "OTHER MODELS";

                                            categoryModels =
                                                models
                                                    .Where(
                                                        x =>
                                                            !x.StartsWith(
                                                                "openai/",
                                                                StringComparison.OrdinalIgnoreCase)
                                                            &&
                                                            !x.StartsWith(
                                                                "qwen/",
                                                                StringComparison.OrdinalIgnoreCase)
                                                            &&
                                                            !(
                                                                x.StartsWith(
                                                                    "google/",
                                                                    StringComparison.OrdinalIgnoreCase)
                                                                &&
                                                                x.Contains(
                                                                    "gemini",
                                                                    StringComparison.OrdinalIgnoreCase)
                                                            )
                                                            &&
                                                            !x.StartsWith(
                                                                "nvidia/",
                                                                StringComparison.OrdinalIgnoreCase)
                                                            &&
                                                            !x.StartsWith(
                                                                "z-ai/",
                                                                StringComparison.OrdinalIgnoreCase)
                                                            &&
                                                            !x.StartsWith(
                                                                "x-ai/",
                                                                StringComparison.OrdinalIgnoreCase)
                                                            &&
                                                            !x.EndsWith(
                                                                ":free",
                                                                StringComparison.OrdinalIgnoreCase)
                                                            &&
                                                            !x.Equals(
                                                                "openrouter/free",
                                                                StringComparison.OrdinalIgnoreCase))
                                                    .ToList();

                                            break;
                                        }

                                        default:
                                        {
                                            ConsoleUI.Error(
                                                "Invalid selection.");

                                            ConsoleUI.Wait();

                                            continue;
                                        }
                                    }

                                    if (categoryModels.Count == 0)
                                    {
                                        Console.Clear();

                                        ConsoleUI.DrawHeader(
                                            categoryName,
                                            "OpenRouter",
                                            currentModel);

                                        Console.WriteLine();

                                        ConsoleUI.Warning(
                                            "No models found in this category.");

                                        ConsoleUI.Wait();

                                        continue;
                                    }

                                    string currentOpenRouterModel =
                                        ConsoleUI.GetOpenRouterModel();

                                    string selectedModel =
                                        ConsoleUI.SelectOpenRouterModel(
                                            categoryModels,
                                            currentOpenRouterModel);

                                    if (string.IsNullOrWhiteSpace(
                                        selectedModel) ||
                                        selectedModel == "None")
                                    {
                                        continue;
                                    }

                                    if (ConsoleUI.SetOpenRouterModel(
                                        selectedModel))
                                    {
                                        currentModel =
                                            selectedModel;

                                        assistant.ConfigureOpenRouter(
                                            ConsoleUI.GetOpenRouterApiKey(),
                                            selectedModel);

                                        ConsoleUI.Success(
                                            "OpenRouter model saved.");

                                        Console.WriteLine();

                                        Console.WriteLine(
                                            "Current Model : " +
                                            selectedModel);

                                        ConsoleUI.Wait();

                                        goto OpenRouterBack;
                                    }

                                    ConsoleUI.Error(
                                        "Cannot save OpenRouter model.");

                                    ConsoleUI.Wait();
                                }
                            }
                            catch (System.Text.Json.JsonException ex)
                            {
                                ConsoleUI.Error(
                                    "Invalid response from OpenRouter: " +
                                    ex.Message);

                                ConsoleUI.Wait();

                                goto OpenRouterBack;
                            }
                            catch (Exception ex)
                            {
                                ConsoleUI.Error(
                                    "Cannot load OpenRouter models: " +
                                    ex.Message);

                                ConsoleUI.Wait();

                                goto OpenRouterBack;
                            }
                        }

                        //--------------------------------------------------
                        // 3 - Test Connection
                        //--------------------------------------------------

                        case "3":
                        {
                            string savedApiKey =
                                ConsoleUI.GetOpenRouterApiKey();

                            if (string.IsNullOrWhiteSpace(
                                savedApiKey))
                            {
                                ConsoleUI.Error(
                                    "Configure an OpenRouter API key first.");

                                ConsoleUI.Wait();

                                goto OpenRouterBack;
                            }

                            Console.Clear();

                            ConsoleUI.DrawHeader(
                                "OpenRouter Connection Test",
                                "OpenRouter",
                                currentModel);

                            Console.WriteLine();

                            Console.ForegroundColor =
                                ConsoleColor.Cyan;

                            Console.WriteLine(
                                " OPENROUTER CONNECTION TEST");

                            Console.ResetColor();

                            Console.WriteLine();

                            Console.WriteLine(
                                "Testing connection...");

                            Console.WriteLine();

                            try
                            {
                                bool connected =
                                    OpenRouter.TestConnectionAsync(
                                        savedApiKey,
                                        ConsoleUI.GetOpenRouterModel())
                                    .GetAwaiter()
                                    .GetResult();

                                Console.WriteLine();

                                if (connected)
                                {
                                    ConsoleUI.Success(
                                        "OpenRouter connection successful.");

                                    Console.WriteLine();

                                    Console.WriteLine(
                                        "API Key : Valid");

                                    Console.WriteLine(
                                        "Server  : Reachable");

                                    Console.WriteLine(
                                        "Status  : Connected");
                                }
                                else
                                {
                                    ConsoleUI.Error(
                                        "OpenRouter connection failed.");
                                }
                            }
                            catch (Exception ex)
                            {
                                ConsoleUI.Error(
                                    "Connection test failed: " +
                                    ex.Message);
                            }

                            ConsoleUI.Wait();

                            goto OpenRouterBack;
                        }

                        //--------------------------------------------------
                        // 4 - Clear API Key
                        //--------------------------------------------------

                        case "4":
                        {
                            if (ConsoleUI.ClearOpenRouterApiKey())
                            {
                                assistant.ConfigureOpenRouter(
                                    "",
                                    ConsoleUI.GetOpenRouterModel());

                                ConsoleUI.Success(
                                    "OpenRouter API key cleared.");
                            }
                            else
                            {
                                ConsoleUI.Error(
                                    "Cannot clear OpenRouter API key.");
                            }

                            ConsoleUI.Wait();

                            goto OpenRouterBack;
                        }

                        //--------------------------------------------------
                        // 0 - Back
                        //--------------------------------------------------

                        case "0":
                        {
                            goto AIProvidersBack;
                        }

                        //--------------------------------------------------
                        // Invalid
                        //--------------------------------------------------

                        default:
                        {
                            ConsoleUI.Error(
                                "Invalid OpenRouter option.");

                            ConsoleUI.Wait();

                            goto OpenRouterBack;
                        }
                    }
                }

                //--------------------------------------------------
                // 3 - Configure AI Providers
                //--------------------------------------------------

                case "3":
                {
                ConfigurationBack:

                    Console.Clear();

                    ConsoleUI.DrawHeader(
                        "Configuration",
                        assistant.ProviderName,
                        currentModel);

                    Console.WriteLine();

                    Console.ForegroundColor =
                        ConsoleColor.Cyan;

                    Console.WriteLine(
                        " CONFIGURE AI PROVIDERS");

                    Console.ResetColor();

                    Console.WriteLine();

                    Console.WriteLine(
                        " [1] OpenRouter Configuration");

                    Console.WriteLine(
                        " [2] Ollama Configuration");

                    Console.WriteLine(
                        " [0] Back");

                    Console.WriteLine();

                    Console.Write(
                        "Select > ");

                    string configurationChoice =
                        Console.ReadLine() ?? "";

                    configurationChoice =
                        configurationChoice.Trim();

                    switch (configurationChoice)
                    {
                        //--------------------------------------------------
                        // OpenRouter Configuration
                        //--------------------------------------------------

                        case "1":
                        {
                        OpenRouterConfigurationBack:

                            Console.Clear();

                            string apiKey =
                                ConsoleUI.GetOpenRouterApiKey();

                            string model =
                                ConsoleUI.GetOpenRouterModel();

                            assistant.ConfigureOpenRouter(
                                apiKey,
                                model);

                            ConsoleUI.ShowOpenRouter(
                                !string.IsNullOrWhiteSpace(apiKey),
                                string.IsNullOrWhiteSpace(model)
                                    ? "None"
                                    : model,
                                string.IsNullOrWhiteSpace(apiKey)
                                    ? "Not configured"
                                    : "Configured");

                            Console.WriteLine();

                            Console.Write(
                                "Select > ");

                            string choice =
                                Console.ReadLine() ?? "";

                            choice =
                                choice.Trim();

                            switch (choice)
                            {
                                //--------------------------------------------------
                                // Set API Key
                                //--------------------------------------------------

                                case "1":
                                {
                                    string newApiKey =
                                        ConsoleUI.ReadOpenRouterApiKey();

                                    if (string.IsNullOrWhiteSpace(
                                        newApiKey))
                                    {
                                        ConsoleUI.Error(
                                            "API key cannot be empty.");

                                        ConsoleUI.Wait();

                                        goto OpenRouterConfigurationBack;
                                    }

                                    if (ConsoleUI.SetOpenRouterApiKey(
                                        newApiKey))
                                    {
                                        assistant.ConfigureOpenRouter(
                                            newApiKey,
                                            ConsoleUI.GetOpenRouterModel());

                                        ConsoleUI.Success(
                                            "OpenRouter API key saved.");
                                    }
                                    else
                                    {
                                        ConsoleUI.Error(
                                            "Cannot save OpenRouter API key.");
                                    }

                                    ConsoleUI.Wait();

                                    goto OpenRouterConfigurationBack;
                                }

                                //--------------------------------------------------
                                // Select Model
                                //--------------------------------------------------

                                case "2":
                                {
                                    ConsoleUI.Warning(
                                        "Use AI Providers -> OpenRouter -> Select Model.");

                                    ConsoleUI.Wait();

                                    goto OpenRouterConfigurationBack;
                                }

                                //--------------------------------------------------
                                // Test Connection
                                //--------------------------------------------------

                                case "3":
                                {
                                    string savedApiKey =
                                        ConsoleUI.GetOpenRouterApiKey();

                                    string savedModel =
                                        ConsoleUI.GetOpenRouterModel();

                                    if (string.IsNullOrWhiteSpace(
                                        savedApiKey))
                                    {
                                        ConsoleUI.ShowOpenRouterConnection(
                                            "Not configured");

                                        ConsoleUI.Wait();

                                        goto OpenRouterConfigurationBack;
                                    }

                                    if (string.IsNullOrWhiteSpace(
                                        savedModel) ||
                                        savedModel == "None")
                                    {
                                        ConsoleUI.ShowOpenRouterConnection(
                                            "No model selected");

                                        ConsoleUI.Wait();

                                        goto OpenRouterConfigurationBack;
                                    }

                                    try
                                    {
                                        assistant.ConfigureOpenRouter(
                                            savedApiKey,
                                            savedModel);

                                        ConsoleUI.ShowOpenRouterConnection(
                                            "Testing connection...");

                                        bool connected =
                                            OpenRouter.TestConnectionAsync(
                                                savedApiKey,
                                                savedModel)
                                            .GetAwaiter()
                                            .GetResult();

                                        ConsoleUI.ShowOpenRouterConnection(
                                            connected
                                                ? "Connected"
                                                : "Connection failed");

                                        ConsoleUI.Wait();

                                        goto OpenRouterConfigurationBack;
                                    }
                                    catch (Exception ex)
                                    {
                                        ConsoleUI.ShowOpenRouterConnection(
                                            "Connection failed: " +
                                            ex.Message);

                                        ConsoleUI.Wait();

                                        goto OpenRouterConfigurationBack;
                                    }
                                }

                                //--------------------------------------------------
                                // Clear API Key
                                //--------------------------------------------------

                                case "4":
                                {
                                    if (ConsoleUI.ClearOpenRouterApiKey())
                                    {
                                        assistant.ConfigureOpenRouter(
                                            "",
                                            ConsoleUI.GetOpenRouterModel());

                                        ConsoleUI.Success(
                                            "OpenRouter API key cleared.");
                                    }
                                    else
                                    {
                                        ConsoleUI.Error(
                                            "Cannot clear OpenRouter API key.");
                                    }

                                    ConsoleUI.Wait();

                                    goto OpenRouterConfigurationBack;
                                }

                                //--------------------------------------------------
                                // Back
                                //--------------------------------------------------

                                case "0":
                                {
                                    goto ConfigurationBack;
                                }

                                //--------------------------------------------------
                                // Invalid
                                //--------------------------------------------------

                                default:
                                {
                                    ConsoleUI.Error(
                                        "Invalid OpenRouter option.");

                                    ConsoleUI.Wait();

                                    goto OpenRouterConfigurationBack;
                                }
                            }
                        }

                        //--------------------------------------------------
                        // Ollama Configuration
                        //--------------------------------------------------

                        case "2":
                        {
                            Console.Clear();

                            ConsoleUI.DrawHeader(
                                "Ollama Configuration",
                                "Ollama",
                                currentModel);

                            Console.WriteLine();

                            Console.ForegroundColor =
                                ConsoleColor.Cyan;

                            Console.WriteLine(
                                " OLLAMA CONFIGURATION");

                            Console.ResetColor();

                            Console.WriteLine();

                            Console.WriteLine(
                                "Ollama uses local AI models.");

                            Console.WriteLine();

                            ConsoleUI.Success(
                                "Ollama configuration is available.");

                            ConsoleUI.Wait();

                            goto ConfigurationBack;
                        }

                        //--------------------------------------------------
                        // Back
                        //--------------------------------------------------

                        case "0":
                        {
                            goto AIProvidersBack;
                        }

                        //--------------------------------------------------
                        // Invalid
                        //--------------------------------------------------

                        default:
                        {
                            ConsoleUI.Error(
                                "Invalid configuration option.");

                            ConsoleUI.Wait();

                            goto ConfigurationBack;
                        }
                    }
                }

                //--------------------------------------------------
                // Back
                //--------------------------------------------------

                case "0":
                {
                    return;
                }

                //--------------------------------------------------
                // Invalid
                //--------------------------------------------------

                default:
                {
                    ConsoleUI.Error(
                        "Invalid AI provider option.");

                    ConsoleUI.Wait();

                    goto AIProvidersBack;
                }
            }

            return;
        }

        //--------------------------------------------------
        // 3 - Help
        //--------------------------------------------------

        case "3":
        {
            ConsoleUI.ShowHelp();

            return;
        }

        //--------------------------------------------------
        // 4 - About
        //--------------------------------------------------

        case "4":
        {
            ConsoleUI.ShowAbout();

            return;
        }

        //--------------------------------------------------
        // 5 - OpenRouter Chat Easter Egg
        //--------------------------------------------------

        case "5":
        {
            OpenRouterChat.RunAsync()
                .GetAwaiter()
                .GetResult();

            return;
        }

        //--------------------------------------------------
        // /menuchat
        //--------------------------------------------------

        case "/menuchat":
        {
            Mode =
                ChatMode.Keyboard;

            ConsoleUI.Success(
                "Keyboard chat enabled.");

            ConsoleUI.Wait();

            return;
        }

        //--------------------------------------------------
        // /voicechat
        //--------------------------------------------------

        case "/voicechat":
        {
            Mode =
                ChatMode.Voice;

            ConsoleUI.Success(
                "Voice chat enabled.");

            ConsoleUI.Wait();

            return;
        }

        //--------------------------------------------------
        // /auto
        //--------------------------------------------------

        case "/auto":
        {
            Mode =
                ChatMode.Auto;

            ConsoleUI.Success(
                "Auto mode enabled.");

            ConsoleUI.Wait();

            return;
        }

        //--------------------------------------------------
        // 0 - Exit
        //--------------------------------------------------

        case "0":
        {
            try
            {
                Dispose();
            }
            catch
            {
                // Preserve shutdown behavior.
            }

            Environment.Exit(0);

            return;
        }

        //--------------------------------------------------
        // Unknown
        //--------------------------------------------------

        default:
        {
            ConsoleUI.Warning(
                "Unknown command.");

            ConsoleUI.Wait();

            return;
        }
    }
}
 private async void StartWatchdog()
{
    watchdogToken?.Cancel();

    watchdogToken = new CancellationTokenSource();

    while (!watchdogToken.IsCancellationRequested)
    {
        try
        {
            // هر دو ثانیه وضعیت را بررسی کن
            await Task.Delay(2000, watchdogToken.Token);

            // اگر در حال ریکاوری هستیم هیچ کاری نکن
            if (recovering)
                continue;

            if (agent == null)
                continue;

            if (!agent.IsCharacterAlive())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[WATCHDOG] Agent crashed.");
                Console.ResetColor();

                recovering = true;

                bool success = RecoverCharacter();
                if(success)
{
    await Task.Delay(10000);

    continue;
}
                recovering = false;

                if (success)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[WATCHDOG] Recovery successful.");
                    Console.WriteLine("[WATCHDOG] Cooldown 10 sec...");
                    Console.ResetColor();

                    // ده ثانیه واچ‌داگ هیچ کاری نکند
                    await Task.Delay(10000, watchdogToken.Token);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[WATCHDOG] Recovery failed.");
                    Console.ResetColor();

                    // پنج ثانیه بعد دوباره امتحان کند
                    await Task.Delay(5000, watchdogToken.Token);
                }
            }
        }
        catch (TaskCanceledException)
        {
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
private void Agent_OnCharacterDead(CrashInfo info)
{
    Console.ForegroundColor = ConsoleColor.Red;

    Console.WriteLine();
    Console.WriteLine("===== AGENT CRASH =====");
    Console.WriteLine($"Reason : {info.Reason}");
    Console.WriteLine($"Method : {info.Method}");
    Console.WriteLine($"Time   : {info.Time}");

    if (info.Exception != null)
        Console.WriteLine(info.Exception.Message);

    Console.ResetColor();

    OnAgentCrash?.Invoke();
}



    //--------------------------------------------------
    // Movings
    //--------------------------------------------------
public async Task Move(int targetX, int targetY)
{
    if (agent == null)
    {
        Console.WriteLine("[MOVE] No agent loaded.");
        return;
    }


    try
    {
        movementToken?.Cancel();

        movementToken =
            new CancellationTokenSource();

        CancellationToken token =
            movementToken.Token;



        int startX = currentX;
        int startY = currentY;



        int dx =
            targetX - startX;

        int dy =
            targetY - startY;



        if (dx == 0 && dy == 0)
            return;



        string animation;



        // انتخاب جهت Animation

        if (Math.Abs(dx) > Math.Abs(dy))
        {
            animation =
                dx > 0
                ? "MoveLeft"
                : "MoveRight";
        }
        else
        {
            animation =
                dy > 0
                ? "MoveDown"
                : "MoveUp";
        }



        isMoving = true;



        // فقط یک بار اجرا
        // Animation خودش کامل اجرا می شود

        agent.Play(animation);



        float x =
            startX;

        float y =
            startY;



        float speed =
            6f;



        while(true)
        {
            token.ThrowIfCancellationRequested();



            float moveX =
                targetX - x;

            float moveY =
                targetY - y;



            float distance =
                MathF.Sqrt(
                    moveX * moveX +
                    moveY * moveY);



            // رسیدن به مقصد

            if(distance <= speed)
                break;



            x +=
                (moveX / distance) *
                speed;


            y +=
                (moveY / distance) *
                speed;



            currentX =
                (int)x;

            currentY =
                (int)y;



            agent.SetPosition(
                (short)currentX,
                (short)currentY);



            await Task.Delay(
                16,
                token);
        }



        // قرار گرفتن دقیق روی مقصد

        currentX =
            targetX;

        currentY =
            targetY;



        agent.SetPosition(
            (short)targetX,
            (short)targetY);



        isMoving = false;



        // پایان حرکت

        agent.Play(
            "RestPose");



        Console.WriteLine(
            $"[MOVE] Finished {targetX},{targetY}");
    }
    catch(TaskCanceledException)
    {
        isMoving = false;

        Console.WriteLine(
            "[MOVE] Cancelled");
    }
    catch(Exception ex)
    {
        isMoving = false;

        Console.WriteLine(
            "[MOVE ERROR]");

        Console.WriteLine(
            ex.Message);
    }
}
    //--------------------------------------------------
    // Dispose
    //--------------------------------------------------

    public void Dispose()
    {
        try
        {
            StopVoiceAsync()
                .GetAwaiter()
                .GetResult();
        }
        catch
        {
        }

        try
        {
            CloseCharacter();
        }
        catch
        {
        }

        assistant.Dispose();
    }
}