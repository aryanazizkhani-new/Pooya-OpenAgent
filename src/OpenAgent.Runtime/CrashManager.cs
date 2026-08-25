namespace OpenAgent.Runtime;

public enum CrashReason
{
    None,

    DaServerClosed,

    RpcUnavailable,

    ComException,

    CharacterLost,

    InvalidAnimation,

    Unknown
}

public sealed class CrashInfo
{
    public CrashReason Reason { get; init; }

    public Exception? Exception { get; init; }

    public string Method { get; init; } = "";

    public DateTime Time { get; init; } = DateTime.Now;
}