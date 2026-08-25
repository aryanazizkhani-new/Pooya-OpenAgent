using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OpenAgent.InteropGenerator;

internal static class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== OpenAgent Interop Generator ===");

        var dllPath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "OpenAgent.InteropWrapper",
            "lib",
            "OpenAgent.Interop.dll");

        dllPath = Path.GetFullPath(dllPath);

        Console.WriteLine("Interop : " + dllPath);

        if (!File.Exists(dllPath))
        {
            Console.WriteLine("OpenAgent.Interop.dll NOT FOUND");
            return;
        }

        var stdOlePath =
            @"C:\Windows\assembly\GAC\stdole\7.0.3300.0__b03f5f7f11d50a3a\stdole.dll";

        if (File.Exists(stdOlePath))
        {
            Console.WriteLine("Loading stdole...");
            Assembly.LoadFrom(stdOlePath);
        }

        var asm = Assembly.LoadFrom(dllPath);

        Type[] types;

        try
        {
            types = asm.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            Console.WriteLine();
            Console.WriteLine("===== LOADER ERRORS =====");

            foreach (var e in ex.LoaderExceptions)
                Console.WriteLine(e?.Message);

            Console.WriteLine();

            types = ex.Types
                .Where(t => t != null)
                .Cast<Type>()
                .ToArray();
        }

        Console.WriteLine();
        Console.WriteLine("===== INTERFACES =====");

        foreach (var t in types.Where(t => t.IsInterface).OrderBy(t => t.FullName))
            Console.WriteLine(t.FullName);

        Console.WriteLine();
        Console.WriteLine("===== IDaServer METHODS =====");

        var server = asm.GetType("OpenAgent.Interop.IDaServer");

        if (server == null)
        {
            Console.WriteLine("IDaServer NOT FOUND");
            return;
        }

        foreach (var method in server.GetMethods())
        {
            Console.WriteLine(method);

            foreach (var parameter in method.GetParameters())
            {
                Console.WriteLine(
                    $"    {parameter.ParameterType.FullName} {parameter.Name}");
            }

            Console.WriteLine();
        }

        Console.WriteLine("Done.");
    }
}