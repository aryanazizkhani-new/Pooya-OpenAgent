using OpenAgent.AI;
using OpenAgent.Runtime;

namespace OpenAgent.Sample;

public sealed class App : IDisposable
{
    //--------------------------------------------------
    // Fields
    //--------------------------------------------------

    private readonly AgentManager manager;

    private string? pendingVoiceText;

    //--------------------------------------------------
    // Constructor
    //--------------------------------------------------

    public App()
    {
        SettingsManager.Load();

        manager = new AgentManager();

        var screen =
            System.Windows.Forms.Screen.PrimaryScreen;

        if (screen != null)
        {
            int width =
                screen.Bounds.Width;

            int height =
                screen.Bounds.Height;

            manager.SetScreenResolution(
                width,
                height);
        }
        else
        {
            manager.SetScreenResolution(
                1920,
                1080);
        }

        manager.OnAgentCrash += RecoverAgent;
    }

    //--------------------------------------------------
    // Run
    //--------------------------------------------------

    public void Run()
    {
        while (true)
        {
            ConsoleUI.Draw(
                "Ready",
                manager.HasAgent
                    ? "Loaded"
                    : "None",
                manager.CurrentAgent != null
                    ? manager.CurrentAgent.CharacterName
                    : "None");

            string? cmd =
                Console.ReadLine();

            switch (cmd)
            {
                case "1":
                {
                    StartAssistant()
                        .GetAwaiter()
                        .GetResult();

                    break;
                }

                default:
                {
                    manager.HandleCommand(cmd);
                    break;
                }
            }
        }
    }

    //--------------------------------------------------
    // Assistant Wizard
    //--------------------------------------------------

    private async Task StartAssistant()
    {
        Console.Clear();

        //--------------------------------------------------
        // Voice Callback
        //--------------------------------------------------

        manager.OnVoiceText += text =>
        {
            pendingVoiceText = text;
        };

        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            "          OPENAGENT SETUP");

        Console.WriteLine(
            "========================================");

        Console.WriteLine();

        //--------------------------------------------------
        // STEP 1
        // Select AI Provider
        //--------------------------------------------------

        Console.WriteLine(
            "[1/6] Select AI Provider");

        Console.WriteLine();

        Console.WriteLine(
            "[1] Ollama");

        Console.WriteLine(
            "[2] OpenRouter");

        Console.WriteLine();

        Console.Write(
            "Select > ");

        string providerChoice =
            Console.ReadLine() ?? "";

        providerChoice =
            providerChoice.Trim();

        //--------------------------------------------------
        // Provider Selection
        //--------------------------------------------------

        if (providerChoice == "1")
        {
            //--------------------------------------------------
            // Ollama
            //--------------------------------------------------

            manager.SetAIProvider(
                AIProvider.Ollama);

            Console.WriteLine();

            Console.WriteLine(
                "[2/6] Select Ollama Model");

            Console.WriteLine();

            List<string> models =
                await manager.GetModelsAsync();

            if (models.Count == 0)
            {
                ConsoleUI.Error(
                    "No Ollama models installed.");

                ConsoleUI.Wait();
                return;
            }

            for (int i = 0;
                 i < models.Count;
                 i++)
            {
                Console.WriteLine(
                    $"[{i + 1}] {models[i]}");
            }

            Console.WriteLine();

            Console.Write(
                "Model > ");

            if (!int.TryParse(
                Console.ReadLine(),
                out int modelIndex))
            {
                ConsoleUI.Error(
                    "Invalid selection.");

                ConsoleUI.Wait();
                return;
            }

            modelIndex--;

            if (modelIndex < 0 ||
                modelIndex >= models.Count)
            {
                ConsoleUI.Error(
                    "Invalid selection.");

                ConsoleUI.Wait();
                return;
            }

            SettingsManager.Current.OllamaModel =
                modelIndex + 1;

            SettingsManager.Save();

            Console.WriteLine();

            Console.WriteLine(
                "Loading model...");

            Console.WriteLine();

            bool ok =
                await manager.LoadModelAsync(
                    models[modelIndex]);

            if (!ok)
            {
                ConsoleUI.Error(
                    "Cannot load model.");

                ConsoleUI.Wait();
                return;
            }

            ConsoleUI.Success(
                $"Loaded Model : {models[modelIndex]}");

            Console.WriteLine();
        }
        else if (providerChoice == "2")
        {
            //--------------------------------------------------
            // OpenRouter
            //--------------------------------------------------

            manager.SetAIProvider(
                AIProvider.OpenRouter);

            bool openRouterReady =
                await ConfigureOpenRouterSetup();

            if (!openRouterReady)
            {
                return;
            }
        }
        else
        {
            ConsoleUI.Error(
                "Invalid provider selection.");

            ConsoleUI.Wait();
            return;
        }

        //--------------------------------------------------
        // STEP 2
        // Searching Microsoft Agents
        //--------------------------------------------------

        Console.WriteLine(
            "[3/6] Searching Microsoft Agents...");

        Console.WriteLine();

        if (!EsSearcher.IsAvailable())
        {
            ConsoleUI.Error(
                "Everything (es.exe) not found.");

            ConsoleUI.Wait();
            return;
        }

        List<AgentInfo> agents =
            EsSearcher.FindAgents();

        if (agents.Count == 0)
        {
            ConsoleUI.Error(
                "No Microsoft Agent (*.acs) found.");

            ConsoleUI.Wait();
            return;
        }

        for (int i = 0;
             i < agents.Count;
             i++)
        {
            Console.WriteLine(
                $"[{i + 1}] {agents[i].Name}");
        }

        Console.WriteLine();

        Console.Write(
            "Character > ");

        if (!int.TryParse(
            Console.ReadLine(),
            out int characterIndex))
        {
            ConsoleUI.Error(
                "Invalid selection.");

            ConsoleUI.Wait();
            return;
        }

        characterIndex--;

        if (characterIndex < 0 ||
            characterIndex >= agents.Count)
        {
            ConsoleUI.Error(
                "Invalid selection.");

            ConsoleUI.Wait();
            return;
        }

        SettingsManager.Current.Agent =
            characterIndex + 1;

        SettingsManager.Save();

        Console.WriteLine();

        //--------------------------------------------------
        // STEP 3
        // Loading Character
        //--------------------------------------------------

        Console.WriteLine(
            "[4/6] Loading Character...");

        Console.WriteLine();

        bool loaded =
            manager.LoadCharacter(
                agents[characterIndex].Path);

        if (!loaded)
        {
            ConsoleUI.Error(
                "Cannot load selected character.");

            ConsoleUI.Wait();
            return;
        }

        ConsoleUI.Success(
            $"Character Loaded : " +
            $"{agents[characterIndex].Name}");

        Console.WriteLine();

        await PlayStartupAsync();

        //--------------------------------------------------
        // STEP 4
        // Select Microphone
        //--------------------------------------------------

        Console.WriteLine(
            "[5/6] Searching Microphones...");

        Console.WriteLine();

        List<string> microphones =
            WhisperSearcher.FindMicrophones();

        if (microphones.Count == 0)
        {
            ConsoleUI.Error(
                "No microphone found.");

            ConsoleUI.Wait();
            return;
        }

        for (int i = 0;
             i < microphones.Count;
             i++)
        {
            Console.WriteLine(
                $"[{i + 1}] {microphones[i]}");
        }

        Console.WriteLine();

        Console.Write(
            "Microphone > ");

        if (!int.TryParse(
            Console.ReadLine(),
            out int microphoneIndex))
        {
            ConsoleUI.Error(
                "Invalid selection.");

            ConsoleUI.Wait();
            return;
        }

        microphoneIndex--;

        if (microphoneIndex < 0 ||
            microphoneIndex >= microphones.Count)
        {
            ConsoleUI.Error(
                "Invalid selection.");

            ConsoleUI.Wait();
            return;
        }

        SettingsManager.Current.Microphone =
            microphoneIndex + 1;

        SettingsManager.Save();

        string microphone =
            microphones[microphoneIndex];

        ConsoleUI.Success(
            $"Selected : {microphone}");

        Console.WriteLine();

        //--------------------------------------------------
        // STEP 5
        // Search Whisper
        //--------------------------------------------------

        Console.WriteLine(
            "[6/6] Searching Whisper...");

        Console.WriteLine();

        string? whisperExe =
            WhisperSearcher.FindExecutable();

        if (string.IsNullOrWhiteSpace(
            whisperExe))
        {
            ConsoleUI.Warning(
                "Whisper Server Not Found");

            Console.WriteLine(
                "Keyboard Chat Enabled");
        }
        else
        {
            ConsoleUI.Success(
                "Whisper Found");

            Console.WriteLine();

            List<string> whisperModels =
                WhisperSearcher.FindModels();

            if (whisperModels.Count == 0)
            {
                ConsoleUI.Warning(
                    "No GGML Models Found");
            }
            else
            {
                Console.WriteLine(
                    "Available Models");

                Console.WriteLine();

                for (int i = 0;
                     i < whisperModels.Count;
                     i++)
                {
                    Console.WriteLine(
                        $"[{i + 1}] " +
                        $"{Path.GetFileName(
                            whisperModels[i])}");
                }

                Console.WriteLine();

                Console.Write(
                    "Model > ");

                if (!int.TryParse(
                    Console.ReadLine(),
                    out int whisperModelIndex))
                {
                    ConsoleUI.Error(
                        "Invalid selection.");

                    ConsoleUI.Wait();
                    return;
                }

                whisperModelIndex--;

                if (whisperModelIndex < 0 ||
                    whisperModelIndex >=
                    whisperModels.Count)
                {
                    ConsoleUI.Error(
                        "Invalid selection.");

                    ConsoleUI.Wait();
                    return;
                }

                SettingsManager.Current.WhisperModel =
                    whisperModelIndex + 1;

                SettingsManager.Save();

                string whisperModel =
                    whisperModels[
                        whisperModelIndex];

                //--------------------------------------------------
                // Search FFmpeg
                //--------------------------------------------------

                Console.WriteLine();

                Console.WriteLine(
                    "Searching FFmpeg...");

                Console.WriteLine();

                List<string> ffmpegList =
                    FFmpegSearcher.FindAll();

                if (ffmpegList.Count == 0)
                {
                    ConsoleUI.Error(
                        "No FFmpeg Found.");

                    ConsoleUI.Wait();
                    return;
                }

                Console.WriteLine(
                    "Available FFmpeg");

                Console.WriteLine();

                for (int i = 0;
                     i < ffmpegList.Count;
                     i++)
                {
                    Console.WriteLine(
                        $"[{i + 1}]");

                    Console.WriteLine(
                        ffmpegList[i]);

                    Console.WriteLine();
                }

                Console.Write(
                    "FFmpeg > ");

                if (!int.TryParse(
                    Console.ReadLine(),
                    out int ffmpegIndex))
                {
                    ConsoleUI.Error(
                        "Invalid Selection.");

                    ConsoleUI.Wait();
                    return;
                }

                ffmpegIndex--;

                if (ffmpegIndex < 0 ||
                    ffmpegIndex >=
                    ffmpegList.Count)
                {
                    ConsoleUI.Error(
                        "Invalid Selection.");

                    ConsoleUI.Wait();
                    return;
                }

                SettingsManager.Current.FFmpeg =
                    ffmpegIndex + 1;

                SettingsManager.Save();

                string ffmpegExe =
                    ffmpegList[ffmpegIndex];

                Console.WriteLine();

                ConsoleUI.Success(
                    "FFmpeg Selected");

                Console.WriteLine(
                    ffmpegExe);

                manager.ConfigureVoice(
                    ffmpegExe,
                    whisperExe,
                    whisperModel,
                    microphone);

                ConsoleUI.Success(
                    "Speech To Text Ready " +
                    "(Type Start for Start voice Chat)");
            }
        }

        Console.WriteLine();

        //--------------------------------------------------
        // STEP 6
        // Chat Loop
        //--------------------------------------------------

        while (true)
        {
            //--------------------------------------------------
            // Voice Text
            //--------------------------------------------------

            if (!string.IsNullOrWhiteSpace(
                pendingVoiceText))
            {
                string text =
                    pendingVoiceText;

                pendingVoiceText = null;

                Console.ForegroundColor =
                    ConsoleColor.Yellow;

                Console.WriteLine(
                    $"You > {text}");

                Console.ResetColor();

                try
                {
                    string answer =
                        await manager.AskAsync(text);

                    Console.ForegroundColor =
                        ConsoleColor.Green;

                    Console.WriteLine();
                    Console.WriteLine(
                        $"AI > {answer}");
                    Console.WriteLine();

                    Console.ResetColor();

                    await manager.Speak(answer);
                }
                catch (Exception ex)
                {
                    ConsoleUI.Error(
                        ex.Message);
                }

                continue;
            }

            //--------------------------------------------------
            // Keyboard
            //--------------------------------------------------

            Console.ForegroundColor =
                ConsoleColor.Yellow;

            Console.Write(
                "You > ");

            Console.ResetColor();

            string? textInput =
                Console.ReadLine();

            if (string.IsNullOrWhiteSpace(
                textInput))
            {
                continue;
            }

            //--------------------------------------------------
            // Commands
            //--------------------------------------------------

            if (textInput.StartsWith("/"))
            {
                manager.HandleCommand(
                    textInput);

                continue;
            }

            //--------------------------------------------------
            // Exit
            //--------------------------------------------------

            if (textInput.Equals(
                "exit",
                StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            //--------------------------------------------------
            // Start Recording
            //--------------------------------------------------

            if (textInput.Equals(
                "start",
                StringComparison.OrdinalIgnoreCase))
            {
                manager.StartVoice();

                ConsoleUI.Success(
                    "Speech To Text Started.");

                continue;
            }

            //--------------------------------------------------
            // Stop Recording
            //--------------------------------------------------

            if (textInput.Equals(
                "stop",
                StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    string? voiceText =
                        await manager.StopVoiceAsync();

                    ConsoleUI.Warning(
                        "Speech To Text Stopped.");

                    if (!string.IsNullOrWhiteSpace(
                        voiceText))
                    {
                        pendingVoiceText =
                            voiceText;
                    }
                }
                catch (Exception ex)
                {
                    ConsoleUI.Error(
                        ex.Message);
                }

                continue;
            }

            //--------------------------------------------------
            // Force Show Agent
            //--------------------------------------------------

            if (textInput.Equals(
                "show",
                StringComparison.OrdinalIgnoreCase))
            {
                manager.ForceShow();

                continue;
            }

            //--------------------------------------------------
            // Keyboard Chat
            //--------------------------------------------------

            try
            {
                string answer =
                    await manager.AskAsync(
                        textInput);

                Console.ForegroundColor =
                    ConsoleColor.Green;

                Console.WriteLine();
                Console.WriteLine(
                    $"AI > {answer}");
                Console.WriteLine();

                Console.ResetColor();

                await manager.Speak(answer);
            }
            catch (Exception ex)
            {
                ConsoleUI.Error(
                    ex.Message);
            }
        }

        //--------------------------------------------------
        // Shutdown Assistant
        //--------------------------------------------------

        Console.WriteLine();

        Console.WriteLine(
            "Closing Assistant...");

        try
        {
            await manager.StopVoiceAsync();
        }
        catch
        {
        }

        manager.CloseCharacter();

        ConsoleUI.Success(
            "Assistant Closed.");

        ConsoleUI.Wait();
    }

    //--------------------------------------------------
    // OpenRouter Setup
    //--------------------------------------------------

    private async Task<bool>
        ConfigureOpenRouterSetup()
    {
        while (true)
        {
            Console.Clear();

            Console.WriteLine(
                "========================================");

            Console.WriteLine(
                "          OPENAGENT SETUP");

            Console.WriteLine(
                "========================================");

            Console.WriteLine();

            Console.WriteLine(
                "[2/6] OpenRouter Setup");

            Console.WriteLine();

            string apiKey =
                ConsoleUI.GetOpenRouterApiKey();

            string model =
                ConsoleUI.GetOpenRouterModel();

            Console.WriteLine(
                "API Key : " +
                (string.IsNullOrWhiteSpace(apiKey)
                    ? "Not configured"
                    : ConsoleUI.MaskOpenRouterApiKey(
                        apiKey)));

            Console.WriteLine(
                "Model   : " +
                (string.IsNullOrWhiteSpace(model)
                    ? "Not selected"
                    : model));

            Console.WriteLine();

            Console.WriteLine(
                "[1] Change API Key");

            Console.WriteLine(
                "[2] Select Model");

            Console.WriteLine(
                "[3] Continue to Setup");

            Console.WriteLine();

            Console.Write(
                "Select > ");

            string choice =
                Console.ReadLine() ?? "";

            choice =
                choice.Trim();

            //--------------------------------------------------
            // Change API Key
            //--------------------------------------------------

            if (choice == "1")
            {
                Console.Clear();

                Console.WriteLine(
                    "========================================");

                Console.WriteLine(
                    "       CHANGE OPENROUTER API KEY");

                Console.WriteLine(
                    "========================================");

                Console.WriteLine();

                if (!string.IsNullOrWhiteSpace(
                    apiKey))
                {
                    Console.WriteLine(
                        "Current API Key : " +
                        ConsoleUI.MaskOpenRouterApiKey(
                            apiKey));

                    Console.WriteLine();
                }

                string newApiKey =
                    ConsoleUI.ReadOpenRouterApiKey();

                if (string.IsNullOrWhiteSpace(
                    newApiKey))
                {
                    ConsoleUI.Error(
                        "API key cannot be empty.");

                    ConsoleUI.Wait();

                    continue;
                }

                if (!ConsoleUI.SetOpenRouterApiKey(
                    newApiKey))
                {
                    ConsoleUI.Error(
                        "Cannot save OpenRouter API key.");

                    ConsoleUI.Wait();

                    continue;
                }

                manager.ConfigureOpenRouter(
                    newApiKey,
                    ConsoleUI.GetOpenRouterModel());

                ConsoleUI.Success(
                    "OpenRouter API key saved.");

                ConsoleUI.Wait();

                continue;
            }

            //--------------------------------------------------
            // Select Model
            //--------------------------------------------------

            if (choice == "2")
            {
                apiKey =
                    ConsoleUI.GetOpenRouterApiKey();

                if (string.IsNullOrWhiteSpace(
                    apiKey))
                {
                    ConsoleUI.Error(
                        "Configure an OpenRouter API key first.");

                    ConsoleUI.Wait();

                    continue;
                }

                string? selectedModel =
                    await SelectOpenRouterModelAsync(
                        apiKey,
                        model);

                if (!string.IsNullOrWhiteSpace(
                    selectedModel))
                {
                    ConsoleUI.SetOpenRouterModel(
                        selectedModel);

                    manager.ConfigureOpenRouter(
                        apiKey,
                        selectedModel);

                    ConsoleUI.Success(
                        $"Selected Model : {selectedModel}");

                    ConsoleUI.Wait();
                }

                continue;
            }

            //--------------------------------------------------
            // Continue
            //--------------------------------------------------

            if (choice == "3")
            {
                apiKey =
                    ConsoleUI.GetOpenRouterApiKey();

                model =
                    ConsoleUI.GetOpenRouterModel();

                if (string.IsNullOrWhiteSpace(
                    apiKey))
                {
                    ConsoleUI.Error(
                        "OpenRouter API key is not configured.");

                    ConsoleUI.Wait();

                    continue;
                }

                if (string.IsNullOrWhiteSpace(
                    model))
                {
                    ConsoleUI.Error(
                        "Select an OpenRouter model first.");

                    ConsoleUI.Wait();

                    continue;
                }

                manager.ConfigureOpenRouter(
                    apiKey,
                    model);

                ConsoleUI.Success(
                    "OpenRouter is ready.");

                Console.WriteLine();

                return true;
            }

            ConsoleUI.Error(
                "Invalid selection.");

            ConsoleUI.Wait();
        }
    }

    //--------------------------------------------------
    // OpenRouter Model Selection
    //--------------------------------------------------

    private async Task<string?>
        SelectOpenRouterModelAsync(
            string apiKey,
            string currentModel)
    {
        try
        {
            Console.Clear();

            Console.WriteLine(
                "========================================");

            Console.WriteLine(
                "        OPENROUTER MODEL SELECT");

            Console.WriteLine(
                "========================================");

            Console.WriteLine();

            Console.WriteLine(
                "Loading models from OpenRouter...");

            Console.WriteLine();

            using var client =
                new HttpClient();

            client.Timeout =
                TimeSpan.FromSeconds(30);

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers
                    .AuthenticationHeaderValue(
                        "Bearer",
                        apiKey);

            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "OpenAgent/1.0");

            HttpResponseMessage response =
                await client.GetAsync(
                    "https://openrouter.ai/api/v1/models");

            string json =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                ConsoleUI.Error(
                    "OpenRouter returned HTTP " +
                    (int)response.StatusCode);

                Console.WriteLine();

                Console.WriteLine(json);

                ConsoleUI.Wait();

                return null;
            }

            using System.Text.Json.JsonDocument document =
                System.Text.Json.JsonDocument.Parse(
                    json);

            if (!document.RootElement.TryGetProperty(
                "data",
                out System.Text.Json.JsonElement data))
            {
                ConsoleUI.Error(
                    "OpenRouter returned an invalid model list.");

                ConsoleUI.Wait();

                return null;
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

                string? id =
                    idElement.GetString();

                if (!string.IsNullOrWhiteSpace(id))
                {
                    models.Add(id);
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

                return null;
            }

            //--------------------------------------------------
            // Categories
            //--------------------------------------------------

            List<string> openAI =
                models
                    .Where(x =>
                        x.StartsWith(
                            "openai/",
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

            List<string> qwen =
                models
                    .Where(x =>
                        x.StartsWith(
                            "qwen/",
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

            List<string> google =
                models
                    .Where(x =>
                        x.StartsWith(
                            "google/",
                            StringComparison.OrdinalIgnoreCase) &&
                        x.Contains(
                            "gemini",
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

            List<string> nvidia =
                models
                    .Where(x =>
                        x.StartsWith(
                            "nvidia/",
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

            List<string> zaiXai =
                models
                    .Where(x =>
                        x.StartsWith(
                            "z-ai/",
                            StringComparison.OrdinalIgnoreCase) ||
                        x.StartsWith(
                            "x-ai/",
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

            List<string> free =
                models
                    .Where(x =>
                        x.Contains(
                            ":free",
                            StringComparison.OrdinalIgnoreCase) ||
                        x.Equals(
                            "openrouter/free",
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

            HashSet<string> categorized =
                new(
                    openAI
                        .Concat(qwen)
                        .Concat(google)
                        .Concat(nvidia)
                        .Concat(zaiXai)
                        .Concat(free),
                    StringComparer.OrdinalIgnoreCase);

            List<string> other =
                models
                    .Where(x =>
                        !categorized.Contains(x))
                    .ToList();

            while (true)
            {
                Console.Clear();

                Console.WriteLine(
                    "========================================");

                Console.WriteLine(
                    "        OPENROUTER MODEL SELECT");

                Console.WriteLine(
                    "========================================");

                Console.WriteLine();

                Console.WriteLine(
                    "[1] OpenAI Models");

                Console.WriteLine(
                    "[2] Qwen Models");

                Console.WriteLine(
                    "[3] Google Gemini Models");

                Console.WriteLine(
                    "[4] NVIDIA Models");

                Console.WriteLine(
                    "[5] Z.ai / xAI Models");

                Console.WriteLine(
                    "[6] Free Models");

                Console.WriteLine(
                    "[7] Other Models");

                Console.WriteLine(
                    "[0] Back");

                Console.WriteLine();

                Console.Write(
                    "Category > ");

                string category =
                    Console.ReadLine() ?? "";

                category =
                    category.Trim();

                List<string>? selectedList =
                    category switch
                    {
                        "1" => openAI,
                        "2" => qwen,
                        "3" => google,
                        "4" => nvidia,
                        "5" => zaiXai,
                        "6" => free,
                        "7" => other,
                        _ => null
                    };

                if (category == "0")
                {
                    return null;
                }

                if (selectedList == null)
                {
                    ConsoleUI.Error(
                        "Invalid category.");

                    ConsoleUI.Wait();

                    continue;
                }

                if (selectedList.Count == 0)
                {
                    ConsoleUI.Warning(
                        "No models found in this category.");

                    ConsoleUI.Wait();

                    continue;
                }

                Console.Clear();

                Console.WriteLine(
                    "========================================");

                Console.WriteLine(
                    "        OPENROUTER MODEL SELECT");

                Console.WriteLine(
                    "========================================");

                Console.WriteLine();

                for (int i = 0;
                     i < selectedList.Count;
                     i++)
                {
                    string marker =
                        string.Equals(
                            selectedList[i],
                            currentModel,
                            StringComparison.OrdinalIgnoreCase)
                            ? "  <current>"
                            : "";

                    Console.WriteLine(
                        $"[{i + 1}] " +
                        $"{selectedList[i]}" +
                        marker);
                }

                Console.WriteLine();

                Console.Write(
                    "Model > ");

                if (!int.TryParse(
                    Console.ReadLine(),
                    out int modelIndex))
                {
                    ConsoleUI.Error(
                        "Invalid selection.");

                    ConsoleUI.Wait();

                    continue;
                }

                modelIndex--;

                if (modelIndex < 0 ||
                    modelIndex >= selectedList.Count)
                {
                    ConsoleUI.Error(
                        "Invalid selection.");

                    ConsoleUI.Wait();

                    continue;
                }

                return selectedList[modelIndex];
            }
        }
        catch (TaskCanceledException)
        {
            ConsoleUI.Error(
                "OpenRouter model request timed out.");

            ConsoleUI.Wait();

            return null;
        }
        catch (HttpRequestException ex)
        {
            ConsoleUI.Error(
                "Network error: " +
                ex.Message);

            ConsoleUI.Wait();

            return null;
        }
        catch (Exception ex)
        {
            ConsoleUI.Error(
                "Cannot load OpenRouter models: " +
                ex.Message);

            ConsoleUI.Wait();

            return null;
        }
    }

    //--------------------------------------------------
    // Startup Animation
    //--------------------------------------------------

    private async Task PlayStartupAsync()
    {
        //----------------------------------
        // Reset AI Memory
        //----------------------------------

        manager.ClearConversation();

        //----------------------------------
        // Small Delay
        //----------------------------------

        await Task.Delay(500);

        //----------------------------------
        // Welcome
        //----------------------------------

        await manager.Speak(
            "Hello my old friend!");

        //----------------------------------
        // Wait Speech
        //----------------------------------

        await Task.Delay(2500);

        //----------------------------------
        // Greeting Animation
        //----------------------------------

        manager.CurrentAgent?.Greet();

        //----------------------------------
        // Return To Idle
        //----------------------------------

        await Task.Delay(4000);

        manager.CurrentAgent?.RestPose();
    }

    //--------------------------------------------------
    // Recovery
    //--------------------------------------------------

    private void RecoverAgent()
    {
        Console.Clear();

        Console.ForegroundColor =
            ConsoleColor.Yellow;

        Console.WriteLine(
            "An error occurred.");

        Console.WriteLine(
            "Recovering Agent...");

        Console.ResetColor();

        if (manager.RecoverCharacter())
        {
            Console.ForegroundColor =
                ConsoleColor.Green;

            Console.WriteLine(
                "Recovery successful.");

            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.WriteLine(
                "Recovery failed.");

            Console.ResetColor();
        }
    }

    //--------------------------------------------------
    // Dispose
    //--------------------------------------------------

    public void Dispose()
    {
        try
        {
            manager.Dispose();
        }
        catch
        {
        }
    }
}