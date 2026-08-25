namespace OpenAgent.AI;

public sealed class AssistantEngine
{
    private readonly OllamaClient client;
    private readonly string model;

    public AssistantEngine(string model)
    {
        this.model = model;
        client = new OllamaClient();
    }

    //--------------------------------------------------------
    // Ask AI
    //--------------------------------------------------------

    public async Task<string> AskAsync(string prompt)
    {
        try
        {
            string answer =
                await client.GenerateAsync(
                    model,
                    prompt);

            return answer;
        }
        catch (Exception ex)
        {
            return $"AI Error : {ex.Message}";
        }
    }

    //--------------------------------------------------------
    // Conversation Loop
    //--------------------------------------------------------

    public async Task RunAsync()
    {
        Console.Clear();

        Console.ForegroundColor = ConsoleColor.Green;

        Console.WriteLine("=======================================");
        Console.WriteLine("        OPENAGENT ASSISTANT");
        Console.WriteLine("=======================================");

        Console.ResetColor();

        Console.WriteLine();
        Console.WriteLine($"Model : {model}");
        Console.WriteLine();

        Console.WriteLine("Type 'exit' to quit.");
        Console.WriteLine();

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("You : ");
            Console.ResetColor();

            string? prompt = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(prompt))
                continue;

            if (prompt.Equals(
                    "exit",
                    StringComparison.OrdinalIgnoreCase))
                break;

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Thinking...");
            Console.ResetColor();

            string answer =
                await AskAsync(prompt);

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Bonzi : ");
            Console.ResetColor();

            Console.WriteLine(answer);

            Console.WriteLine();
        }
    }
}