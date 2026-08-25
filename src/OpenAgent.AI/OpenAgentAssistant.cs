using OpenAgent.Runtime;

namespace OpenAgent.AI;

public sealed class OpenAgentAssistant : IDisposable
{
    //--------------------------------------------------
    // Fields
    //--------------------------------------------------

    private readonly AgentBrain brain;

    private readonly WhisperStream whisper;

    private AgentLoader? agent;

    //--------------------------------------------------
    // Voice Configuration
    //--------------------------------------------------

    private string ffmpegExe = "";

    private string whisperExe = "";

    private string whisperModel = "";
    private string microphoneName = "";

    //--------------------------------------------------
    // Voice Event
    //--------------------------------------------------

    public event Action<string>? OnVoiceText;

    //--------------------------------------------------
    // Constructor
    //--------------------------------------------------

    public OpenAgentAssistant()
    {
        brain = new AgentBrain();

        whisper = new WhisperStream();

        whisper.OnText += text =>
        {
            OnVoiceText?.Invoke(text);
        };
    }

    //--------------------------------------------------
    // Agent
    //--------------------------------------------------

    public void AttachAgent(
        AgentLoader loader)
    {
        agent = loader;
    }

    public void DetachAgent()
    {
        agent = null;
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
    this.ffmpegExe =
        ffmpegExe;

    this.whisperExe =
        whisperExe;

    this.whisperModel =
        whisperModel;

    microphoneName =
        microphone;
}
 
    public void StartVoice()
    {
whisper.Start(
    ffmpegExe,
    whisperExe,
    whisperModel,
    microphoneName);
    }

//--------------------------------------------------
// Stop Voice
//--------------------------------------------------

public async Task<string?> StopVoiceAsync()
{
    try
    {
        string? text =
            await whisper.StopAsync();

        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        text =
            text.Trim();

        OnVoiceText?.Invoke(text);

        return text;
    }
    catch (Exception ex)
    {
        Console.WriteLine();
        Console.WriteLine("===== ASSISTANT VOICE ERROR =====");
        Console.WriteLine(ex.ToString());
        Console.WriteLine("=================================");

        return null;
    }
}

    //--------------------------------------------------
    // Models
    //--------------------------------------------------

    public async Task<List<string>> GetInstalledModelsAsync()
    {
        return await brain.Models.GetModelsAsync();
    }

    public async Task<bool> LoadModelAsync(
        string model)
    {
        return await brain.LoadModelAsync(model);
    }

    public async Task<bool> DownloadModelAsync(
        string model,
        Action<string>? progress = null)
    {
        return await brain.DownloadModelAsync(
            model,
            progress);
    }

    //--------------------------------------------------
    // AI
    //--------------------------------------------------

    public async Task<string> AskAsync(
        string message)
    {
        return await brain.AskAsync(message);
    }

    public async Task StreamAsync(
        string message,
        Action<string> onToken)
    {
        await brain.StreamAsync(
            message,
            onToken);
    }
        //--------------------------------------------------
    // Speak
    //--------------------------------------------------

    public async Task SpeakAsync(
        string message)
    {
        if (agent == null)
            return;

Console.WriteLine(
"[ASSISTANT] Speak requested");

await brain.SpeakAsync(
    agent,
    message);
    }

    //--------------------------------------------------
    // Streaming Speak
    //--------------------------------------------------

    public async Task SpeakStreamingAsync(
        string message)
    {
        if (agent == null)
            return;

        await brain.SpeakStreamingAsync(
            agent,
            message);
    }

    //--------------------------------------------------
    // Immediate Speak
    //--------------------------------------------------

    public void Speak(
        string text)
    {
        if (agent == null)
            return;

        agent.Stop();

        agent.Speak(text);
    }

    //--------------------------------------------------
    // Conversation
    //--------------------------------------------------

    public void ClearChat()
    {
        brain.ClearChat();
    }

    public string ExportChat()
    {
        return brain.ExportChat();
    }

    //--------------------------------------------------
    // Stop AI
    //--------------------------------------------------

    public void Stop()
    {
        brain.Stop();
    }
        //--------------------------------------------------
    // Status
    //--------------------------------------------------

    public bool IsModelRunning
    {
        get
        {
            return brain.IsRunning;
        }
    }

    public string CurrentModel
    {
        get
        {
            return brain.CurrentModel;
        }
    }

    public bool IsVoiceRunning
    {
        get
        {
            return whisper.IsRunning;
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
            Stop();
        }
        catch
        {
        }

        try
        {
            whisper.Dispose();
        }
        catch
        {
        }

        try
        {
            brain.Dispose();
        }
        catch
        {
        }

        agent = null;
    }
}