using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace OpenAgent.AI;

public sealed class OllamaClient : IDisposable
{
    //--------------------------------------------------
    // Fields
    //--------------------------------------------------

    private readonly HttpClient http;

    private const string Endpoint =
        "http://127.0.0.1:11434/api/generate";

    //--------------------------------------------------
    // Constructor
    //--------------------------------------------------

    public OllamaClient()
    {
        http = new HttpClient();

        http.Timeout =
            Timeout.InfiniteTimeSpan;
    }

    //--------------------------------------------------
    // Generate (Normal)
    //--------------------------------------------------

    public async Task<string> GenerateAsync(
        string model,
        string prompt)
    {
        var body = new
        {
            model = model,
            prompt = prompt,
            stream = false
        };

        string json =
            JsonSerializer.Serialize(body);

        using StringContent content =
            new(
                json,
                Encoding.UTF8,
                "application/json");

        using HttpResponseMessage response =
            await http.PostAsync(
                Endpoint,
                content);

        response.EnsureSuccessStatusCode();

        string responseText =
            await response.Content
                .ReadAsStringAsync();

        using JsonDocument doc =
            JsonDocument.Parse(responseText);

        if (doc.RootElement.TryGetProperty(
                "response",
                out JsonElement value))
        {
            return value.GetString() ?? "";
        }

        return "";
    }

    //--------------------------------------------------
    // Streaming
    //--------------------------------------------------
        public async IAsyncEnumerable<string> GenerateStreamAsync(
        string model,
        string prompt)
    {
        var body = new
        {
            model = model,
            prompt = prompt,
            stream = true
        };

        string json =
            JsonSerializer.Serialize(body);

        using StringContent content =
            new(
                json,
                Encoding.UTF8,
                "application/json");

        using HttpRequestMessage request =
            new(HttpMethod.Post, Endpoint);

        request.Content = content;

        using HttpResponseMessage response =
            await http.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        await using Stream stream =
            await response.Content.ReadAsStreamAsync();

        using StreamReader reader =
            new(stream);

        while (true)
        {
            string? line =
                await reader.ReadLineAsync();

            if (line == null)
                yield break;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            JsonDocument doc =
                JsonDocument.Parse(line);

            if (doc.RootElement.TryGetProperty(
                "response",
                out JsonElement token))
            {
                string? text =
                    token.GetString();

                if (!string.IsNullOrWhiteSpace(text))
                    yield return text;
            }

            if (doc.RootElement.TryGetProperty(
                "done",
                out JsonElement done))
            {
                if (done.GetBoolean())
                    yield break;
            }
        }
    }

    //--------------------------------------------------
    // Ping Ollama
    //--------------------------------------------------
        public async Task<bool> IsRunningAsync()
    {
        try
        {
            using HttpResponseMessage response =
                await http.GetAsync(
                    "http://127.0.0.1:11434/api/tags");

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    //--------------------------------------------------
    // Installed Models
    //--------------------------------------------------

    public async Task<List<string>> GetModelsAsync()
    {
        List<string> models = new();

        using HttpResponseMessage response =
            await http.GetAsync(
                "http://127.0.0.1:11434/api/tags");

        response.EnsureSuccessStatusCode();

        string json =
            await response.Content
                .ReadAsStringAsync();

        using JsonDocument doc =
            JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty(
                "models",
                out JsonElement array))
        {
            return models;
        }

        foreach (JsonElement item in
                 array.EnumerateArray())
        {
            if (item.TryGetProperty(
                    "name",
                    out JsonElement name))
            {
                string? model =
                    name.GetString();

                if (!string.IsNullOrWhiteSpace(model))
                    models.Add(model);
            }
        }

        return models;
    }

    //--------------------------------------------------
    // Dispose
    //--------------------------------------------------
        //--------------------------------------------------
    // Version
    //--------------------------------------------------

    public async Task<string> GetVersionAsync()
    {
        try
        {
            using HttpResponseMessage response =
                await http.GetAsync(
                    "http://127.0.0.1:11434/api/version");

            response.EnsureSuccessStatusCode();

            string json =
                await response.Content
                    .ReadAsStringAsync();

            using JsonDocument doc =
                JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty(
                    "version",
                    out JsonElement version))
            {
                return version.GetString() ?? "";
            }

            return "";
        }
        catch
        {
            return "";
        }
    }

    //--------------------------------------------------
    // Dispose
    //--------------------------------------------------

    public void Dispose()
    {
        http.Dispose();

        GC.SuppressFinalize(this);
    }
}