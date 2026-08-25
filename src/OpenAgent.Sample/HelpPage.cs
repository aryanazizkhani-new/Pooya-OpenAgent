namespace OpenAgent.Sample;
using OpenAgent.AI;

public static class HelpPage
{
    public static void Show()
    {
        Console.Clear();

        Console.ForegroundColor = ConsoleColor.Green;

        Console.WriteLine("==========================================");
        Console.WriteLine("              OPENAGENT HELP");
        Console.WriteLine("==========================================");

        Console.ResetColor();

        Console.WriteLine();

        Console.WriteLine("ABOUT");
        Console.WriteLine("------------------------------------------");
        Console.WriteLine("OpenAgent is an Open Source AI Desktop");
        Console.WriteLine("Assistant powered by Double Agent and");
        Console.WriteLine("Ollama.");
        Console.WriteLine();

        Console.WriteLine("HOW TO USE");
        Console.WriteLine("------------------------------------------");
        Console.WriteLine("1. Install Double Agent");
        Console.WriteLine("2. Install Ollama");
        Console.WriteLine("3. Install Everything");
        Console.WriteLine("4. Start Assistant");
        Console.WriteLine();

        Console.WriteLine("FEATURES");
        Console.WriteLine("------------------------------------------");
        Console.WriteLine("✔ Automatic Agent Detection");
        Console.WriteLine("✔ Local AI");
        Console.WriteLine("✔ Offline");
        Console.WriteLine("✔ Plugin Ready");
        Console.WriteLine();

        Console.WriteLine("Version");
        Console.WriteLine("------------------------------------------");
        Console.WriteLine("OpenAgent 0.1 Alpha");
        Console.WriteLine();

        Console.WriteLine("Press ENTER...");
        Console.ReadLine();
    }
}