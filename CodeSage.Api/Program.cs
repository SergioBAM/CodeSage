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

var app = builder.Build();

app.MapGet("/health", () =>
{
    return Results.Ok(new { status = "gumba!"});
});

app.Run();

// needed for webapplication factory in tests.
public partial class Program { }