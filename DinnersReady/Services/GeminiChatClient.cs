using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace DinnersReady.Services;

public class GeminiChatClient(string apiKey, string model = "gemini-1.5-flash") : IChatClient
{
    private readonly HttpClient _httpClient = new();

    public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        var promptBuilder = new StringBuilder();
        foreach (var msg in chatMessages)
        {
            promptBuilder.AppendLine($"{msg.Role}: {msg.Text}");
        }

        var requestBody = new GeminiRequest
        {
            Contents = new List<GeminiContent>
            {
                new GeminiContent
                {
                    Parts = new List<GeminiPart>
                    {
                        new GeminiPart { Text = promptBuilder.ToString() }
                    }
                }
            }
        };

        string jsonPayload = JsonSerializer.Serialize(requestBody, DinnersReadyJsonContext.Default.GeminiRequest);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Gemini API Error ({(int)response.StatusCode}): {responseJson}");
        }

        using var doc = JsonDocument.Parse(responseJson);
        var responseText = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return new ChatResponse(new ChatMessage(ChatRole.Assistant, responseText));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = await GetResponseAsync(chatMessages, options, cancellationToken);

        yield return new ChatResponseUpdate(ChatRole.Assistant, response.Text);
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => this;
    public void Dispose() => _httpClient.Dispose();
}