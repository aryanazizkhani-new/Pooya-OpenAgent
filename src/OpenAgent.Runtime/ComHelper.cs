using System.Runtime.InteropServices;

namespace OpenAgent.Runtime;

internal static class ComHelper
{
    public static object Create(Guid clsid)
    {
        var type = Type.GetTypeFromCLSID(clsid)
            ?? throw new Exception("COM class not found.");

        return Activator.CreateInstance(type)
            ?? throw new Exception("Cannot create COM object.");
    }
}