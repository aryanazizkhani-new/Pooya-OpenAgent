namespace OpenAgent.Runtime;

public sealed class AgentInfo
{
    public string Name { get; set; } = "";

    public string Path { get; set; } = "";

    public override string ToString()
    {
        return Name;
    }
}