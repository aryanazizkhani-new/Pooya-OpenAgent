using System.Diagnostics;
using System.Net.Http;

namespace OpenAgent.AI;

public static class OllamaManager
{
    private const string OllamaExe = "ollama";

    public static async Task<bool> IsRunningAsync()
    {
        try
        {
            using HttpClient client = new();

            client.Timeout = TimeSpan.FromSeconds(2);

            HttpResponseMessage response =
                await client.GetAsync("http://127.0.0.1:11434/api/tags");

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool> StartAsync()
    {
        if (await IsRunningAsync())
            return true;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = OllamaExe,
                Arguments = "serve",
                CreateNoWindow = true,
                UseShellExecute = false
            });

            for (int i = 0; i < 20; i++)
            {
                if (await IsRunningAsync())
                    return true;

                await Task.Delay(500);
            }
        }
        catch
        {
        }

        return false;
    }
}