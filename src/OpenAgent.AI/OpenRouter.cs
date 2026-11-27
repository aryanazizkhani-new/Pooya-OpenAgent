using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAgent.AI
{
    public static class OpenRouter
    {
        private const string Endpoint =
            "https://openrouter.ai/api/v1/chat/completions";

        private static readonly HttpClient Http = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();

            client.Timeout = TimeSpan.FromSeconds(120);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            return client;
        }

        /// <summary>
        /// Sends a message to OpenRouter.
        /// </summary>
        public static async Task<string> AskAsync(
            string apiKey,
            string model,
            string message,
            string? systemPrompt = null,
            CancellationToken cancellationToken = default)
        {
            Validate(apiKey, model, message);

            var messages = string.IsNullOrWhiteSpace(systemPrompt)
                ? new object[]
                {
                    new
                    {
                        role = "user",
                        content = message
                    }
                }
                : new object[]
                {
                    new
                    {
                        role = "system",
                        content = systemPrompt
                    },
                    new
                    {
                        role = "user",
                        content = message
                    }
                };

            var request = new
            {
                model,
                messages
            };

            string json = JsonSerializer.Serialize(
                request,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }
            );

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                Endpoint
            );

            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    apiKey
                );

            httpRequest.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            using HttpResponseMessage response =
                await Http.SendAsync(
                    httpRequest,
                    HttpCompletionOption.ResponseContentRead,
                    cancellationToken
                );

            string responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken
                );

            if (!response.IsSuccessStatusCode)
            {
                throw new OpenRouterException(
                    $"OpenRouter returned HTTP {(int)response.StatusCode} " +
                    $"({response.StatusCode}).",
                    response.StatusCode,
                    responseBody
                );
            }

            return ExtractContent(responseBody);
        }

        /// <summary>
        /// Tests the API connection using the supplied key and model.
        /// </summary>
        public static async Task<bool> TestConnectionAsync(
            string apiKey,
            string model,
            CancellationToken cancellationToken = default)
        {
            try
            {
                string result = await AskAsync(
                    apiKey,
                    model,
                    "Reply with exactly: OpenAgent OK",
                    cancellationToken: cancellationToken
                );

                return !string.IsNullOrWhiteSpace(result);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a detailed connection status.
        /// </summary>
        public static async Task<string> GetConnectionStatusAsync(
            string apiKey,
            string model,
            CancellationToken cancellationToken = default)
        {
            try
            {
                string result = await AskAsync(
                    apiKey,
                    model,
                    "Reply with exactly: OpenAgent OK",
                    cancellationToken: cancellationToken
                );

                if (string.IsNullOrWhiteSpace(result))
                {
                    return "Connected, but the server returned an empty response.";
                }

                return "Connected successfully.";
            }
            catch (OpenRouterException ex)
            {
                return $"OpenRouter error: {ex.Message}";
            }
            catch (TaskCanceledException)
            {
                return "Connection timed out or was cancelled.";
            }
            catch (HttpRequestException ex)
            {
                return $"Network error: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Unexpected error: {ex.Message}";
            }
        }

        private static void Validate(
            string apiKey,
            string model,
            string message)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException(
                    "OpenRouter API key is required.",
                    nameof(apiKey)
                );
            }

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new ArgumentException(
                    "OpenRouter model is required.",
                    nameof(model)
                );
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException(
                    "Message cannot be empty.",
                    nameof(message)
                );
            }
        }

        private static string ExtractContent(string json)
        {
            try
            {
                using JsonDocument document =
                    JsonDocument.Parse(json);

                JsonElement root = document.RootElement;

                if (!root.TryGetProperty(
                        "choices",
                        out JsonElement choices))
                {
                    throw new OpenRouterException(
                        "OpenRouter response does not contain 'choices'.",
                        null,
                        json
                    );
                }

                if (choices.ValueKind != JsonValueKind.Array ||
                    choices.GetArrayLength() == 0)
                {
                    throw new OpenRouterException(
                        "OpenRouter returned an empty choices array.",
                        null,
                        json
                    );
                }

                JsonElement choice = choices[0];

                if (!choice.TryGetProperty(
                        "message",
                        out JsonElement message))
                {
                    throw new OpenRouterException(
                        "OpenRouter response does not contain 'message'.",
                        null,
                        json
                    );
                }

                if (!message.TryGetProperty(
                        "content",
                        out JsonElement content))
                {
                    throw new OpenRouterException(
                        "OpenRouter response does not contain 'content'.",
                        null,
                        json
                    );
                }

                if (content.ValueKind == JsonValueKind.String)
                {
                    return content.GetString() ?? string.Empty;
                }

                return content.ToString();
            }
            catch (JsonException ex)
            {
                throw new OpenRouterException(
                    "OpenRouter returned invalid JSON.",
                    null,
                    json,
                    ex
                );
            }
        }
    }

    public sealed class OpenRouterException : Exception
    {
        public System.Net.HttpStatusCode? StatusCode { get; }

        public string ResponseBody { get; }

        public OpenRouterException(
            string message,
            System.Net.HttpStatusCode? statusCode,
            string responseBody,
            Exception? innerException = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}