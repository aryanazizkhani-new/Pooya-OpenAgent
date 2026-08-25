using OpenAgent.Interop;
using System.Runtime.InteropServices;

namespace OpenAgent.Runtime;

public sealed class AgentHost : IDisposable
{
    public IDaServer2 Server { get; }

    public AgentHost()
    {
        Type? type = Type.GetTypeFromProgID("DoubleAgent.Server");

        if (type == null)
            throw new Exception(
                "Double Agent Server is not installed.");

        Server =
            (IDaServer2)Activator.CreateInstance(type)!;
    }

    //--------------------------------------------------
    // Load Character
    //--------------------------------------------------

    public IDaSvrCharacter LoadCharacter(
        string file,
        out int characterId)
    {
        int request;

        Server.Load(
            file,
            out characterId,
            out request);

        Server.GetCharacterEx(
            characterId,
            out IDaSvrCharacter character);

        return character;
    }

    //--------------------------------------------------
    // Unload Character
    //--------------------------------------------------

    public void UnloadCharacter(int id)
    {
        try
        {
            Server.Unload(id);
        }
        catch
        {
        }
    }

    //--------------------------------------------------
    // Dispose
    //--------------------------------------------------

public void Dispose()
{
    try
    {
        if (Server != null)
        {
            while (Marshal.ReleaseComObject(Server) > 0)
            {
            }
        }
    }
    catch
    {
    }

    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();

    GC.SuppressFinalize(this);
}
}