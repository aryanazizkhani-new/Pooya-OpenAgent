using OpenAgent.Runtime;

namespace OpenAgent.AI;

public enum AIProvider
{
    Ollama,
    OpenRouter
}

public sealed class OpenAgentAssistant : IDisposable
{
    //--------------------------------------------------
    // Fields
    //--------------------------------------------------

    private readonly AgentBrain brain;

    private readonly WhisperStream whisper;

    private AgentLoader? agent;

    //--------------------------------------------------
    // AI Provider
    //--------------------------------------------------

    private AIProvider provider =
        AIProvider.Ollama;

    //--------------------------------------------------
    // OpenRouter
    //--------------------------------------------------

    private string openRouterApiKey = "";

    private string openRouterModel = "";

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
    // AI Provider
    //--------------------------------------------------

    public AIProvider Provider
    {
        get
        {
            return provider;
        }
    }

    public string ProviderName
    {
        get
        {
            return provider switch
            {
                AIProvider.Ollama =>
                    "Ollama",

                AIProvider.OpenRouter =>
                    "OpenRouter",

                _ =>
                    "Unknown"
            };
        }
    }

    public void SetProvider(
        AIProvider selectedProvider)
    {
        provider =
            selectedProvider;
    }

    //--------------------------------------------------
    // OpenRouter Configuration
    //--------------------------------------------------

    public void ConfigureOpenRouter(
        string apiKey,
        string model)
    {
        openRouterApiKey =
            apiKey?.Trim() ?? "";

        openRouterModel =
            model?.Trim() ?? "";
    }

    //--------------------------------------------------
    // OpenRouter Status
    //--------------------------------------------------

    public bool IsOpenRouterConfigured
    {
        get
        {
            return
                !string.IsNullOrWhiteSpace(
                    openRouterApiKey) &&
                !string.IsNullOrWhiteSpace(
                    openRouterModel);
        }
    }

    public string OpenRouterModel
    {
        get
        {
            return openRouterModel;
        }
    }

    //--------------------------------------------------
    // Selected Provider AI
    //--------------------------------------------------

    public async Task<string> AskSelectedProviderAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(
                "Message cannot be empty.",
                nameof(message));
        }

        switch (provider)
        {
            case AIProvider.Ollama:
            {
                return await AskAsync(message);
            }

            case AIProvider.OpenRouter:
            {
                return await AskOpenRouterAsync(
                    message,
                    null,
                    cancellationToken);
            }

            default:
            {
                throw new InvalidOperationException(
                    "Unknown AI provider.");
            }
        }
    }

    //--------------------------------------------------
    // OpenRouter Chat
    //--------------------------------------------------

    public async Task<string> AskOpenRouterAsync(
        string message,
        string? systemPrompt = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(
                openRouterApiKey))
        {
            throw new InvalidOperationException(
                "OpenRouter API key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(
                openRouterModel))
        {
            throw new InvalidOperationException(
                "OpenRouter model is not configured.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(
                "Message cannot be empty.",
                nameof(message));
        }

        return await OpenRouter.AskAsync(
            openRouterApiKey,
            openRouterModel,
            message,
            systemPrompt,
            cancellationToken);
    }

    //--------------------------------------------------
    // Voice Configuration
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

    //--------------------------------------------------
    // Start Voice
    //--------------------------------------------------

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

            Console.WriteLine(
                "===== ASSISTANT VOICE ERROR =====");

            Console.WriteLine(
                ex.ToString());

            Console.WriteLine(
                "=================================");

            return null;
        }
    }

    //--------------------------------------------------
    // Ollama Models
    //--------------------------------------------------

    public async Task<List<string>>
        GetInstalledModelsAsync()
    {
        return await brain.Models.GetModelsAsync();
    }

    public async Task<bool> LoadModelAsync(
        string model)
    {
        return await brain.LoadModelAsync(
            model);
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
    // Ollama AI
    //--------------------------------------------------

    public async Task<string> AskAsync(
        string message)
    {
        return await brain.AskAsync(
            message);
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
        {
            return;
        }

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
        {
            return;
        }

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
        {
            return;
        }

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