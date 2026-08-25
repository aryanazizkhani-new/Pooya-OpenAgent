using OpenAgent.Runtime;

namespace OpenAgent.Sample;

public sealed class App : IDisposable
{
    //--------------------------------------------------
    // Fields
    //--------------------------------------------------

    private readonly AgentManager manager;

    //--------------------------------------------------
    // Constructor
    //--------------------------------------------------

public App()
{
    SettingsManager.Load();

    manager = new AgentManager();

    var screen = System.Windows.Forms.Screen.PrimaryScreen;

    if (screen != null)
    {
        int width = screen.Bounds.Width;
        int height = screen.Bounds.Height;

        manager.SetScreenResolution(
            width,
            height);
    }
    else
    {
        // مقدار پیش‌فرض اگر نتوانست صفحه را پیدا کند
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
                manager.HasAgent ? "Loaded" : "None",
                manager.CurrentAgent != null
                    ? manager.CurrentAgent.CharacterName
                    : "None");

            string? cmd =
                Console.ReadLine();

            switch (cmd)
            {
                case "1":
                {
                    StartAssistant().Wait();
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
//--------------------------------------------------
// Voice Buffer
//--------------------------------------------------

private string? pendingVoiceText;
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

    Console.WriteLine("========================================");
    Console.WriteLine("          OPENAGENT SETUP");
    Console.WriteLine("========================================");
    Console.WriteLine();

    //--------------------------------------------------
    // STEP 1
    //--------------------------------------------------

    Console.WriteLine("[1/6] Select AI Model");
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

    for (int i = 0; i < models.Count; i++)
    {
        Console.WriteLine(
            $"[{i + 1}] {models[i]}");
    }

    Console.WriteLine();

    Console.Write("Model > ");

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
    SettingsManager.Current.OllamaModel =
    modelIndex + 1;

SettingsManager.Save();

    if (modelIndex < 0 ||
        modelIndex >= models.Count)
    {
        ConsoleUI.Error(
            "Invalid selection.");

        ConsoleUI.Wait();
        return;
    }

    Console.WriteLine();
    Console.WriteLine("Loading model...");
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

    //--------------------------------------------------
    // STEP 2
    //--------------------------------------------------

    Console.WriteLine("[2/6] Searching Microsoft Agents...");
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

    for (int i = 0; i < agents.Count; i++)
    {
        Console.WriteLine(
            $"[{i + 1}] {agents[i].Name}");
    }

    Console.WriteLine();

    Console.Write("Character > ");

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
    SettingsManager.Current.Agent =
    characterIndex + 1;

SettingsManager.Save();

    if (characterIndex < 0 ||
        characterIndex >= agents.Count)
    {
        ConsoleUI.Error(
            "Invalid selection.");

        ConsoleUI.Wait();
        return;
    }

    Console.WriteLine();

    //--------------------------------------------------
    // STEP 3
    //--------------------------------------------------

    Console.WriteLine("[3/6] Loading Character...");
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
        $"Character Loaded : {agents[characterIndex].Name}");

    Console.WriteLine();
    await PlayStartupAsync();
    //------------------------------------------
// STEP 4
// Select Microphone
//------------------------------------------

Console.WriteLine("[4/6] Searching Microphones...");
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

for (int i = 0; i < microphones.Count; i++)
{
    Console.WriteLine(
        $"[{i + 1}] {microphones[i]}");
}

Console.WriteLine();

Console.Write("Microphone > ");

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
SettingsManager.Current.Microphone =
    microphoneIndex + 1;

SettingsManager.Save();

if (microphoneIndex < 0 ||
    microphoneIndex >= microphones.Count)
{
    ConsoleUI.Error(
        "Invalid selection.");

    ConsoleUI.Wait();
    return;
}

string microphone =
    microphones[microphoneIndex];

ConsoleUI.Success(
    $"Selected : {microphone}");

Console.WriteLine();
                //------------------------------------------
        // STEP 5
        // Search Whisper
        //------------------------------------------

        Console.WriteLine("[5/6] Searching Whisper...");
        Console.WriteLine();

        string? whisperExe =
            WhisperSearcher.FindExecutable();

        if (string.IsNullOrWhiteSpace(whisperExe))
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

                for (int i = 0; i < whisperModels.Count; i++)
                {
                    Console.WriteLine(
                        $"[{i + 1}] {Path.GetFileName(whisperModels[i])}");
                }

                Console.WriteLine();

                Console.Write("Model > ");
int whisperModelIndex =
    int.Parse(Console.ReadLine()!);

whisperModelIndex--;
SettingsManager.Current.WhisperModel =
    whisperModelIndex + 1;

SettingsManager.Save();



string whisperModel =
    whisperModels[whisperModelIndex];

//--------------------------------------------------
// Search FFmpeg
//--------------------------------------------------

Console.WriteLine();
Console.WriteLine("Searching FFmpeg...");
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

Console.WriteLine("Available FFmpeg");
Console.WriteLine();

for (int i = 0; i < ffmpegList.Count; i++)
{
    Console.WriteLine(
        $"[{i + 1}]");

    Console.WriteLine(
        ffmpegList[i]);

    Console.WriteLine();
}

Console.Write("FFmpeg > ");

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
SettingsManager.Current.FFmpeg =
    ffmpegIndex + 1;

SettingsManager.Save();

if (ffmpegIndex < 0 ||
    ffmpegIndex >= ffmpegList.Count)
{
    ConsoleUI.Error(
        "Invalid Selection.");

    ConsoleUI.Wait();

    return;
}

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
                    "Speech To Text Ready(Type Start for Start voice Chat)");
            }
        }

        Console.WriteLine();
//------------------------------------------
// STEP 6
// Chat Loop
//------------------------------------------


        //------------------------------------------
        // Chat Loop
        //------------------------------------------

        while (true)
        {
            //--------------------------------------------------
            // اگر Whisper متنی آماده کرده
            //--------------------------------------------------

            if (!string.IsNullOrWhiteSpace(pendingVoiceText))
            {
                string text =
                    pendingVoiceText;

                pendingVoiceText = null;

                Console.ForegroundColor =
                    ConsoleColor.Yellow;

                Console.WriteLine($"You > {text}");

                Console.ResetColor();
                

                try
                {
                    string answer =
                        await manager.AskAsync(text);

                    Console.ForegroundColor =
                        ConsoleColor.Green;

                    Console.WriteLine();
                    Console.WriteLine($"AI > {answer}");
                    Console.WriteLine();

                    Console.ResetColor();
                    await manager.Speak(answer);
                }
                catch (Exception ex)
                {
                    ConsoleUI.Error(ex.Message);
                }

                continue;
            }

            //--------------------------------------------------
            // Keyboard
            //--------------------------------------------------

            Console.ForegroundColor =
                ConsoleColor.Yellow;

            Console.Write("You > ");

            Console.ResetColor();
            

            string? textInput =
                Console.ReadLine();
                if (string.IsNullOrWhiteSpace(textInput))
    continue;

if (textInput.StartsWith("/"))
{
    manager.HandleCommand(textInput);
    continue;
}

            if (string.IsNullOrWhiteSpace(textInput))
                continue;

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

                    if (!string.IsNullOrWhiteSpace(voiceText))
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
        await manager.AskAsync(textInput);

    Console.ForegroundColor =
        ConsoleColor.Green;

    Console.WriteLine();
    Console.WriteLine($"AI > {answer}");
    Console.WriteLine();

    Console.ResetColor();

    await manager.Speak(answer);
}
catch (Exception ex)
{
    ConsoleUI.Error(ex.Message);
}
        }
                //------------------------------------------
        // Shutdown Assistant
        //------------------------------------------

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

    await manager.Speak("Hello my old friend!");

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
private void RecoverAgent()
{
    Console.Clear();

    Console.ForegroundColor = ConsoleColor.Yellow;

    Console.WriteLine("An error occurred.");
    Console.WriteLine("Recovering Agent...");

    Console.ResetColor();

    if (manager.RecoverCharacter())
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Recovery successful.");
        Console.ResetColor();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Recovery failed.");
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