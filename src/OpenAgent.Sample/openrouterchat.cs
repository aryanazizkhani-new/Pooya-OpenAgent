using System;
using System.Net.Http;
using System.Threading.Tasks;
using OpenAgent.AI;

namespace OpenAgent.Sample;

public static class OpenRouterChat
{
    public static async Task RunAsync()
    {
        Console.Clear();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("======================================");
        Console.WriteLine("        OPENROUTER CHAT TEST");
        Console.WriteLine("======================================");
        Console.ResetColor();

        string apiKey = ConsoleUI.GetOpenRouterApiKey();
        string model = ConsoleUI.GetOpenRouterModel();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            ConsoleUI.Error("OpenRouter API key is not configured.");
            ConsoleUI.Wait();
            return;
        }

        if (string.IsNullOrWhiteSpace(model) ||
            model.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            ConsoleUI.Error("No OpenRouter model is selected.");
            ConsoleUI.Wait();
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"Model : {model}");
        Console.WriteLine("Type /exit to leave the chat.");
        Console.WriteLine();

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("You : ");
            Console.ResetColor();

            string? message = Console.ReadLine();

            if (message == null)
            {
                break;
            }

            message = message.Trim();

            if (message.Length == 0)
            {
                continue;
            }

            if (message.Equals(
                "/exit",
                StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            try
            {
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("AI  : ");
                Console.ResetColor();

                string response =
                    await OpenRouter.AskAsync(
                        apiKey,
                        model,
                        message);

                Console.WriteLine(response);
                Console.WriteLine();
            }
            catch (OpenRouterException ex)
            {
                Console.WriteLine();

                ConsoleUI.Error(
                    $"OpenRouter error: {ex.Message}");

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Response body:");
                Console.WriteLine("--------------------------------------");
                Console.WriteLine(ex.ResponseBody);
                Console.WriteLine("--------------------------------------");
                Console.ResetColor();

                Console.WriteLine();
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine();

                ConsoleUI.Error(
                    "The request timed out or was cancelled.");

                Console.WriteLine();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine();

                ConsoleUI.Error(
                    $"Network error: {ex.Message}");

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine();

                ConsoleUI.Error(
                    $"Unexpected error: {ex.Message}");

                Console.WriteLine();
            }
        }

        Console.WriteLine();
        ConsoleUI.Success("OpenRouter chat closed.");
        ConsoleUI.Wait();
    }
}