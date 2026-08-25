using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenAgent.AI;

public sealed class ChatSession
{
    //--------------------------------------------------
    // Message
    //--------------------------------------------------

    public sealed class ChatMessage
    {
        public string Role { get; }

        public string Content { get; }

        public DateTime Time { get; init; }

        public ChatMessage(
            string role,
            string content)
        {
            Role = role;
            Content = content;
            Time = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{Role}: {Content}";
        }
        [JsonConstructor]
public ChatMessage(
    string role,
    string content,
    DateTime time)
{
    Role = role;
    Content = content;
    Time = time;
}
    }

    //--------------------------------------------------
    // Fields
    //--------------------------------------------------

    private readonly List<ChatMessage> history =
        new();

    //--------------------------------------------------
    // Properties
    //--------------------------------------------------

    public IReadOnlyList<ChatMessage> History =>
        history;

    public int Count =>
        history.Count;

    //--------------------------------------------------
    // Add User Message
    //--------------------------------------------------

    public void AddUser(string text)
    {
        history.Add(
            new ChatMessage(
                "User",
                text));
    }

    //--------------------------------------------------
    // Add Assistant Message
    //--------------------------------------------------

    public void AddAssistant(string text)
    {
        history.Add(
            new ChatMessage(
                "Assistant",
                text));
    }

    //--------------------------------------------------
    // Add System Message
    //--------------------------------------------------

    public void AddSystem(string text)
    {
        history.Add(
            new ChatMessage(
                "System",
                text));
    }

    //--------------------------------------------------
    // Last Message
    //--------------------------------------------------

    public ChatMessage? LastMessage
    {
        get
        {
            if (history.Count == 0)
                return null;

            return history[^1];
        }
    }

    //--------------------------------------------------
    // Remove Last
    //--------------------------------------------------

    public void RemoveLast()
    {
        if (history.Count == 0)
            return;

        history.RemoveAt(history.Count - 1);
    }
        //--------------------------------------------------
    // Clear Chat
    //--------------------------------------------------

    public void Clear()
    {
        history.Clear();
    }

    //--------------------------------------------------
    // Build Prompt
    //--------------------------------------------------

    public string BuildPrompt()
    {
        if (history.Count == 0)
            return string.Empty;

        StringBuilder builder = new();

        foreach (ChatMessage msg in history)
        {
            builder.AppendLine(
                $"{msg.Role}: {msg.Content}");
        }

        builder.AppendLine();
        builder.Append("Assistant:");

        return builder.ToString();
    }

    //--------------------------------------------------
    // Export Chat
    //--------------------------------------------------

    public string Export()
    {
        if (history.Count == 0)
            return string.Empty;

        StringBuilder builder = new();

        foreach (ChatMessage msg in history)
        {
            builder.AppendLine(
                $"[{msg.Time:HH:mm:ss}] {msg.Role}");

            builder.AppendLine(msg.Content);

            builder.AppendLine();
        }

        return builder.ToString();
    }

    //--------------------------------------------------
    // Import Chat
    //--------------------------------------------------

    public void Import(IEnumerable<ChatMessage> messages)
    {
        history.Clear();

        foreach (ChatMessage msg in messages)
        {
            history.Add(msg);
        }
    }

    //--------------------------------------------------
    // Find Messages
    //--------------------------------------------------

    public IEnumerable<ChatMessage> FindByRole(
        string role)
    {
        return history.Where(x =>
            x.Role.Equals(
                role,
                StringComparison.OrdinalIgnoreCase));
    }

    //--------------------------------------------------
    // Contains Text
    //--------------------------------------------------

    public bool Contains(string text)
    {
        return history.Any(x =>
            x.Content.Contains(
                text,
                StringComparison.OrdinalIgnoreCase));
    }
    //--------------------------------------------------
// Export Json
//--------------------------------------------------

public string ExportJson()
{
    return JsonSerializer.Serialize(
        history,
        new JsonSerializerOptions
        {
            WriteIndented = true
        });
}

//--------------------------------------------------
// Save Json
//--------------------------------------------------

public void SaveJson(string file)
{
    File.WriteAllText(
        file,
        ExportJson());
}

//--------------------------------------------------
// Import Json
//--------------------------------------------------

public void ImportJson(string json)
{
    history.Clear();

    List<ChatMessage>? messages =
        JsonSerializer.Deserialize<List<ChatMessage>>(json);

    if (messages == null)
        return;

    history.AddRange(messages);
}

//--------------------------------------------------
// Load Json
//--------------------------------------------------

public void LoadJson(string file)
{
    if (!File.Exists(file))
        return;

    ImportJson(
        File.ReadAllText(file));
}
}