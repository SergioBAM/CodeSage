using CodeSage.Api.Data;
using CodeSage.Api.Services;
using CodeSage.Core.Embedding;
using Microsoft.EntityFrameworkCore;
using System.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default"),
        o => o.UseVector()
    );
});

var ollamaEndpoint = builder.Configuration["Ollama:Endpoint"] ?? "http://localhost:11434";

builder.Services.AddOllamaEmbeddingGenerator(
    modelId: "nomic-embed-text",
    endpoint: new Uri(ollamaEndpoint)
);

builder.Services.AddAntiforgery();
builder.Services.AddHttpClient<OllamaChatService>();

builder.Services.AddScoped<IEmbeddingService, SemanticKernelEmbeddingService>();
builder.Services.AddScoped<RagQueryService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

// Endpoints.
app.MapGet("/", () =>
{
    return Results.Redirect("/index.html");
});

app.MapGet("/health", () =>
{
    return Results.Ok(new { status = "gumba!"});
});

app.MapPost("/query", async (QueryRequest request, RagQueryService rag, OllamaChatService chat) =>
{
    if (string.IsNullOrWhiteSpace(request.Question))
    {
        return Results.BadRequest("Question is required.");
    }

    var chunks = await rag.FindRelevantChunksAsync(request.Question);
    var answer = await chat.AskAsync(request.Question, chunks.Select(c => c.Content));

    return Results.Ok(new
    {
       question = request.Question,
       answer,
       sources = chunks.Select(c => new
       {
           c.FilePath,
           c.ChunkIndex           
       }) 
    });
});

app.MapPost("/ui/query", async (
    HttpRequest request,
    RagQueryService rag,
    OllamaChatService chat) =>
{
    var form = await request.ReadFormAsync();
    var question = form["question"].ToString().Trim();

    if (string.IsNullOrWhiteSpace(question))
    {
        return Results.Content("", "text/html");
    }

    var chunks = await rag.FindRelevantChunksAsync(question);
    var answer = await chat.AskAsync(question, chunks.Select(c => c.Content));
    var answerHtml = Markdig.Markdown.ToHtml(answer);

    var sources = chunks
        .Select(c => $"""<span class="source-badge">{c.FilePath} · chunk {c.ChunkIndex}</span>""")
        .ToList();
    
    var sourcesHtml = sources.Count > 0
        ? $"""<div class="sources">{string.Join("", sources)}</div>"""
        : "";

    var html = $"""
        <div class="bubble-row" id="thinking">
          <div class="avatar bot">CS</div>
          <div>
            <div class="bubble">{answerHtml}</div>
            {sourcesHtml}
          </div>
        </div>
        """;
        
    return Results.Content(html, "text/html");
});

app.Run();

// needed for webapplication factory in tests.
public partial class Program { }

record QueryRequest(string Question);