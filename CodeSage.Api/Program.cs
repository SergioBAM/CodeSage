using CodeSage.Api.Data;
using CodeSage.Core.Embedding;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddScoped<IEmbeddingService, SemanticKernelEmbeddingService>();
builder.Services.AddScoped<RagQueryService>();

var app = builder.Build();

// Endpoints.
app.MapGet("/health", () =>
{
    return Results.Ok(new { status = "gumba!"});
});

app.MapPost("/query", async (QueryRequest request, RagQueryService rag) =>
{
    if (string.IsNullOrWhiteSpace(request.Question))
    {
        return Results.BadRequest("Question is required.");
    }

    var chunks = await rag.FindRelevantChunksAsync(request.Question);

    return Results.Ok(new
    {
       question = request.Question,
       chunks = chunks.Select(c => new
       {
           c.FilePath,
           c.ChunkIndex,
           c.Content
       }) 
    });
});

app.Run();

// needed for webapplication factory in tests.
public partial class Program { }

record QueryRequest(string Question);