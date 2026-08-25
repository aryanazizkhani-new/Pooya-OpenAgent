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
                await assistant.AskAsync(text);

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
    return await assistant.AskAsync(message);
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
    // Console Commands
    //--------------------------------------------------

    public void HandleCommand(
        string? command)
    {
        if (command != null &&
    command.StartsWith("/play "))
{
    if (agent == null)
        return;

    string animation =
        command.Substring(6);

    agent.Play(animation);

    return;
}
//--------------------------------------------------
// Move Commands
//--------------------------------------------------

if (command != null &&
    command.StartsWith("/move", StringComparison.OrdinalIgnoreCase))
{
    string data = command.Substring(5).Trim();









    //--------------------------------------------------
    // /move x y
    //--------------------------------------------------

    string[] args =
        data.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries);

    var pos = MoveCommand.Parse(args);

    if (pos != null)
    {
        Move(
            pos.Value.x,
            pos.Value.y);
    }
    else
    {
        Console.WriteLine("[MOVE] Invalid command.");
    }

    return;
}
        
        switch (command)
        {
            
            //--------------------------------------------------
            // Load AI Model
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
                    GetModelsAsync().Result;

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
                        models[index]).Result;

                if (ok)
                {
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
            // Help
            //--------------------------------------------------

            case "2":
            {
                ConsoleUI.ShowHelp();
                return;
            }

            //--------------------------------------------------
            // About
            //--------------------------------------------------

            case "3":
            {
                ConsoleUI.ShowAbout();
                return;
            }

            //--------------------------------------------------
            // Keyboard Chat
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
            // Voice Chat
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
            // Auto Chat
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
            // Exit
            //--------------------------------------------------

            case "0":
            {
                Dispose();

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