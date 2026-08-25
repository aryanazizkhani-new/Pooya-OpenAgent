using System.Text;

namespace OpenAgent.AI;

public sealed class StreamingResponse
{
    //--------------------------------------------------
    // Fields
    //--------------------------------------------------

    private readonly StringBuilder builder =
        new();

    private bool running;

    //--------------------------------------------------
    // Properties
    //--------------------------------------------------

    public bool IsRunning => running;

    public string Text => builder.ToString();

    public int Length => builder.Length;

    //--------------------------------------------------
    // Start
    //--------------------------------------------------

    public void Start()
    {
        builder.Clear();

        running = true;
    }

    //--------------------------------------------------
    // Append Token
    //--------------------------------------------------

    public void Append(string token)
    {
        if (!running)
            return;

        if (string.IsNullOrWhiteSpace(token))
            return;

        builder.Append(token);
    }

    //--------------------------------------------------
    // Append Line
    //--------------------------------------------------

    public void AppendLine(string line)
    {
        if (!running)
            return;

        builder.AppendLine(line);
    }

    //--------------------------------------------------
    // Stop
    //--------------------------------------------------

    public void Stop()
    {
        running = false;
    }

    //--------------------------------------------------
    // Clear
    //--------------------------------------------------

    public void Clear()
    {
        builder.Clear();

        running = false;
    }

    //--------------------------------------------------
    // Events
    //--------------------------------------------------

    public event Action<string>? TokenReceived;

    public event Action<string>? Completed;

    public event Action? Cancelled;

    //--------------------------------------------------
    // Push Token
    //--------------------------------------------------

    public void Push(string token)
    {
        if (!running)
            return;

        Append(token);

        TokenReceived?.Invoke(token);
    }

    //--------------------------------------------------
    // Finish
    //--------------------------------------------------

    public void Finish()
    {
        if (!running)
            return;

        running = false;

        Completed?.Invoke(builder.ToString());
    }

    //--------------------------------------------------
    // Cancel
    //--------------------------------------------------

    public void Cancel()
    {
        if (!running)
            return;

        running = false;

        Cancelled?.Invoke();
    }

    //--------------------------------------------------
    // Get Balloon Text
    //--------------------------------------------------

    public string GetBalloonText(
        int maxLength = 300)
    {
        string text = builder.ToString();

        if (text.Length <= maxLength)
            return text;

        return text[..maxLength] + "...";
    }

    //--------------------------------------------------
    // Get Speak Text
    //--------------------------------------------------

    public string GetSpeakText()
    {
        return builder.ToString();
    }

    //--------------------------------------------------
    // Override
    //--------------------------------------------------

    public override string ToString()
    {
        return builder.ToString();
    }
}