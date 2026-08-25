namespace OpenAgent.Sample;

public static class ConsoleUI
{
    //--------------------------------------------------
    // Header
    //--------------------------------------------------

    public static void DrawHeader(
        string status = "Ready",
        string character = "None",
        string model = "None")
    {
        Console.ForegroundColor = ConsoleColor.Green;

        Console.WriteLine("""
   ____                  ___                     __
  / __ \____  ___  ____ /   | ____ ____  ____  / /_
 / / / / __ \/ _ \/ __ / /| |/ __ `/ _ \/ __ \/ __/
/ /_/ / /_/ /  __/ / / / ___ / /_/ /  __/ / / / /_
\____/ .___/\___/_/ /_/_/  |_\__, /\___/_/ /_/\__/
    /_/                     /____/

                OpenAgent v1.0
""");

        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.DarkGray;

        Console.WriteLine("==============================================================");
        Console.WriteLine($" Status : {status,-20} Character : {character}");
        Console.WriteLine($" Model  : {model}");
        Console.WriteLine("==============================================================");

        Console.ResetColor();

        Console.WriteLine();
    }

    //--------------------------------------------------
    // Main Menu
    //--------------------------------------------------

//--------------------------------------------------
// Main Menu
//--------------------------------------------------

public static void Draw(
    string status = "Ready",
    string character = "None",
    string model = "None")
{
    Console.Clear();

    DrawHeader(status, character, model);

    Console.ForegroundColor = ConsoleColor.Cyan;

    Console.WriteLine(" MAIN MENU");

    Console.ResetColor();

    Console.WriteLine();

    Console.WriteLine(" [1] Start Assistant");
    Console.WriteLine(" [2] Help");
    Console.WriteLine(" [3] About");
    Console.WriteLine(" [0] Exit");

    Console.WriteLine();

    Console.ForegroundColor = ConsoleColor.DarkGray;

    Console.WriteLine("--------------------------------------------------------------");

    Console.ResetColor();

    Console.Write("Select > ");
}
        //--------------------------------------------------
    // AI Models
    //--------------------------------------------------

    public static void ShowModels(
        List<string> models,
        string currentModel = "None")
    {
        Console.Clear();

        DrawHeader(
            "Ready",
            "None",
            currentModel);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(" AI MODELS");
        Console.ResetColor();

        Console.WriteLine();

        Console.WriteLine($" Current Model : {currentModel}");

        Console.WriteLine();

        Console.WriteLine($" Installed Models : {models.Count}");

        Console.WriteLine();

        for (int i = 0; i < models.Count; i++)
        {
            Console.WriteLine($" [{i + 1}] {models[i]}");
        }

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("--------------------------------------------------------------");
        Console.ResetColor();

        Console.WriteLine(" [D] Download Model");
        Console.WriteLine(" [R] Refresh");
        Console.WriteLine(" [0] Back");

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("==============================================================");
        Console.ResetColor();

        Console.Write("Select > ");
    }

    //--------------------------------------------------
    // Load Character
    //--------------------------------------------------

    public static void ShowLoadCharacter(
        string model = "None")
    {
        Console.Clear();

        DrawHeader(
            "Ready",
            "None",
            model);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(" LOAD CHARACTER");
        Console.ResetColor();

        Console.WriteLine();

        Warning(
            "It is recommended to run ONE AI Model and ONE Agent.");

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("--------------------------------------------------------------");
        Console.ResetColor();
    }
        //--------------------------------------------------
    // Help
    //--------------------------------------------------

    public static void ShowHelp()
{
    Console.Clear();

    Console.ForegroundColor = ConsoleColor.Cyan;

    Console.WriteLine("OPENAGENT HELP");
    Console.WriteLine("====================================");

    Console.ResetColor();

    Console.WriteLine();

    Console.WriteLine("[1] Start Assistant");

    Console.WriteLine("[2] Voice");
    Console.WriteLine("[3] Moving");
    Console.WriteLine("[4] Animations");
    Console.WriteLine("[5] Conversation");
    Console.WriteLine("[6] About");

    Console.WriteLine();

    Console.Write("Select : ");

    string? input = Console.ReadLine();

    switch (input)
    {
        case "1":
            ShowHelpAssistant();
            break;



        case "2":
            ShowHelpVoice();
            break;

        case "3":
            ShowHelpMoving();
            break;

        case "4":
            ShowHelpAnimations();
            break;

        case "5":
            ShowHelpConversation();
            break;

        case "6":
            ShowAbout();
            break;

        default:
            return;
    }
}
private static void ShowHelpMoving()
{
    Console.Clear();

    Console.ForegroundColor = ConsoleColor.Yellow;

    Console.WriteLine("MOVING HELP");
    Console.WriteLine("====================================");

    Console.ResetColor();

    Console.WriteLine();

    Console.WriteLine("Coordinate Mode");
    Console.WriteLine("----------------");
    Console.WriteLine("/move 100 200");
    Console.WriteLine("/move left 100 200");
    Console.WriteLine("/move right 300 250");

    Console.WriteLine();

    Console.WriteLine("Simple Mode");
    Console.WriteLine("----------------");
    Console.WriteLine("/move center");
    Console.WriteLine("/move left");
    Console.WriteLine("/move right");
    Console.WriteLine("/move top");
    Console.WriteLine("/move bottom");
    Console.WriteLine("/move top-left");
    Console.WriteLine("/move top-right");
    Console.WriteLine("/move bottom-left");
    Console.WriteLine("/move bottom-right");

    Console.WriteLine();

    Console.WriteLine("Examples");
    Console.WriteLine("----------------");
    Console.WriteLine("/move center");
    Console.WriteLine("/move left");
    Console.WriteLine("/move right 150 220");

    Console.WriteLine();

    Console.Write("Press ENTER...");
    Console.ReadLine();
}
private static void ShowHelpAssistant()
{
    Console.Clear();

    Console.WriteLine("ASSISTANT HELP");
    Console.WriteLine("------------------------");

    Console.WriteLine();

    Console.WriteLine("Start chatting normally.");
    Console.WriteLine("Example:");
    Console.WriteLine("Hello");
    Console.WriteLine("How are you?");
    Console.WriteLine("Tell me a joke");

    Console.WriteLine();

    Console.Write("Press ENTER...");
    Console.ReadLine();
}
private static void ShowHelpVoice()
{
    Console.Clear();

    Console.WriteLine("VOICE HELP");
    Console.WriteLine("------------------------");

    Console.WriteLine();

    Console.WriteLine("/voicechat");
    Console.WriteLine("/menuchat");
    Console.WriteLine("/auto");

    Console.WriteLine();

    Console.Write("Press ENTER...");
    Console.ReadLine();
}
private static void ShowHelpAnimations()
{
    Console.Clear();

    Console.WriteLine("ANIMATION HELP");
    Console.WriteLine("------------------------");

    Console.WriteLine();

    Console.WriteLine("/play Explain");
    Console.WriteLine("/play Think");
    Console.WriteLine("/play Read");
    Console.WriteLine("/play GestureLeft");
    Console.WriteLine("/play GestureRight");

    Console.WriteLine();

    Console.Write("Press ENTER...");
    Console.ReadLine();
}
private static void ShowHelpConversation()
{
    Console.Clear();

    Console.WriteLine("CONVERSATION HELP");
    Console.WriteLine("------------------------");

    Console.WriteLine();

    Console.WriteLine("Normal chat:");
    Console.WriteLine("Hello");
    Console.WriteLine("What is AI?");
    Console.WriteLine("Explain Physics");

    Console.WriteLine();

    Console.Write("Press ENTER...");
    Console.ReadLine();
}
    //--------------------------------------------------
    // About
    //--------------------------------------------------

    public static void ShowAbout()
    {
        Console.Clear();

        DrawHeader();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("ABOUT OPENAGENT");
        Console.WriteLine("==============================================================");
        Console.ResetColor();

        Console.WriteLine();

        Console.WriteLine("Project");
        Console.WriteLine("   OpenAgent");

        Console.WriteLine();

        Console.WriteLine("Version");
        Console.WriteLine("   1.0");

        Console.WriteLine();

        Console.WriteLine("Framework");
        Console.WriteLine("   .NET 10");

        Console.WriteLine();

        Console.WriteLine("Features");
        Console.WriteLine("   • Microsoft Agent");
        Console.WriteLine("   • Double Agent");
        Console.WriteLine("   • Ollama AI");
        Console.WriteLine("   • Everything Search");
        Console.WriteLine("   • Offline AI");

        Console.WriteLine();

        Console.WriteLine("Developer");
        Console.WriteLine("   Aryan Azizkhani");

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("==============================================================");
        Console.ResetColor();

        Wait();
    }

    //--------------------------------------------------
    // Messages
    //--------------------------------------------------

    public static void Success(string text)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[OK] " + text);
        Console.ResetColor();
    }

    public static void Error(string text)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[ERROR] " + text);
        Console.ResetColor();
    }

    public static void Warning(string text)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[WARNING] " + text);
        Console.ResetColor();
    }

    //--------------------------------------------------
    // Wait
    //--------------------------------------------------

    public static void Wait()
    {
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("Press any key to continue...");
        Console.ResetColor();

        Console.ReadKey(true);
    }
}