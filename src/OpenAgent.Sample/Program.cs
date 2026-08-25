using System;

namespace OpenAgent.Sample;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        Console.Title = "OpenAgent v2.0";

        App app = new();

        try
        {
            app.Run();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine();
            Console.WriteLine("Fatal Error");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine(ex.Message);

            Console.ResetColor();

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");

            Console.ReadKey(true);
        }
        finally
        {
            app.Dispose();
        }
    }
}