using OpenAgent.Runtime;

namespace OpenAgent.AI;

public sealed class AgentBrain : IDisposable
{
    //--------------------------------------------------
    // Fields
    //--------------------------------------------------

    private readonly ModelManager modelManager;

    private readonly ChatSession session;

    private readonly OllamaClient client;

    //--------------------------------------------------
    // Properties
    //--------------------------------------------------

    public ModelManager Models =>
        modelManager;

    public ChatSession Session =>
        session;

    public bool IsRunning =>
        modelManager.IsRunning;

    public string CurrentModel =>
        modelManager.CurrentModel;

    //--------------------------------------------------
    // Constructor
    //--------------------------------------------------

    public AgentBrain()
    {
        modelManager =
            new ModelManager();

        session =
            new ChatSession();

        client =
            new OllamaClient();
    }

    //--------------------------------------------------
    // Model
    //--------------------------------------------------

    public async Task<bool> LoadModelAsync(
        string model)
    {
        if (!await modelManager
            .ModelExistsAsync(model))
        {
            return false;
        }

        return modelManager
            .RunModel(model);
    }

    //--------------------------------------------------
    // Download
    //--------------------------------------------------

    public async Task<bool> DownloadModelAsync(
        string model,
        Action<string>? progress = null)
    {
        return await modelManager
            .DownloadModelAsync(
                model,
                progress);
    }
        //--------------------------------------------------
    // Ask
    //--------------------------------------------------

    public async Task<string> AskAsync(
        string userText)
    {
        session.AddUser(userText);

        string prompt =
            session.BuildPrompt();

        string answer =
            await client.GenerateAsync(
                modelManager.CurrentModel,
                prompt);

        if (string.IsNullOrWhiteSpace(answer))
            answer = "...";

        session.AddAssistant(answer);

        return answer;
    }

//--------------------------------------------------
// Stream
//--------------------------------------------------

public async Task StreamAsync(
    string userText,
    Action<string> onToken)
{
    session.AddUser(userText);

    string prompt =
        session.BuildPrompt();

    string complete = "";
    string buffer = "";

    await foreach (
        string token in
        client.GenerateStreamAsync(
            modelManager.CurrentModel,
            prompt))
    {
        complete += token;
        buffer += token;

        if (buffer.Length >= 120 ||
            buffer.EndsWith(".") ||
            buffer.EndsWith("!") ||
            buffer.EndsWith("?") ||
            buffer.EndsWith("\n"))
        {
            onToken(buffer);
            buffer = "";
        }
    }

    if (buffer.Length > 0)
    {
        onToken(buffer);
    }

    session.AddAssistant(complete);
}
    //--------------------------------------------------
    // Clear Conversation
    //--------------------------------------------------

    public void ClearChat()
    {
        session.Clear();
    }

    //--------------------------------------------------
    // Export Conversation
    //--------------------------------------------------

    public string ExportChat()
    {
        return session.Export();
    }
        //--------------------------------------------------
    // Agent Speak
    //--------------------------------------------------

public Task SpeakAsync(
    AgentLoader agent,
    string text)
{
    if (agent == null)
        return Task.CompletedTask;

    agent.Stop();


    agent.Speak(text);

    return Task.CompletedTask;
}

    //--------------------------------------------------
    // Agent Speak Streaming
    //--------------------------------------------------

public Task SpeakStreamingAsync(
    AgentLoader agent,
    string text)
{
    if (agent == null)
        return Task.CompletedTask;

    agent.Stop();

    agent.Speak(text);

    return Task.CompletedTask;
}

    //--------------------------------------------------
    // Stop
    //--------------------------------------------------

    public void Stop()
    {
        modelManager.StopModel();
    }

    //--------------------------------------------------
    // Dispose
    //--------------------------------------------------

    public void Dispose()
    {
        try
        {
            Stop();
        }
        catch
        {
        }

        modelManager.Dispose();

        client.Dispose();
    }
}