using System.Runtime.InteropServices;

namespace OpenAgent.Runtime;

public static class AgentRecovery
{
    public static bool IsRpcError(COMException ex)
    {
        return ex.ErrorCode == unchecked((int)0x800706BA) || // RPC_S_SERVER_UNAVAILABLE
               ex.ErrorCode == unchecked((int)0x80010108);    // RPC_E_DISCONNECTED
    }
}