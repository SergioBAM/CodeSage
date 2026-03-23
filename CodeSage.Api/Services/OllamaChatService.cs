using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace CodeSage.Api.Services;

public sealed class OllamaChatService
{
    private readonly HttpClient _http;
    private readonly string _model;
    private readonly string _endpoint;    

    public OllamaChatService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _model = config["Ollama:ChatModel"] ?? "llama3.2:3b";
        _endpoint = config["Ollama:Endpoint"] ?? "";        
    }

    public async Task<string> AskAsync(string question, IEnumerable<string> contextChunks,
        CancellationToken ct = default)
    {
        var context = string.Join("\n\n---\n\n", contextChunks);
        var prompt = $"""
            You are a helpful assistant that answers questions about a codebase.
            Use only the context below to answer. If the answer isn't in the context, say so.

            ## Context
            {context}

            ## Question
            {question}
            """;

        var payload = new
        {
            model = _model,
            stream = false,
            messages = new[]
            {
                new { role = "user", content = prompt}
            }
        };

        var response = await _http.PostAsJsonAsync(
            $"{_endpoint}/api/chat", payload, ct);
        
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return json
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;
    }
}