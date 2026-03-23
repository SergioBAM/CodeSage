using CodeSage.Core.Chunking;
using CodeSage.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Pgvector;
using Microsoft.Extensions.AI;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                "Host=localhost;Port=5432;Database=codesage;Username=codesage;Password=codesage",
                o => o.UseVector()));

        services.AddKernel()
            .AddOllamaEmbeddingGenerator(
                modelId: "nomic-embed-text",                
                endpoint: new Uri("http://localhost:11434"));

        services.AddSingleton<ITextChunker, SlidingWindowChunker>();
    })
    .Build();

var db = host.Services.GetRequiredService<AppDbContext>();
var chunker = host.Services.GetRequiredService<ITextChunker>();
var embeddingService = host.Services
    .GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

var targetPath = args.FirstOrDefault() ?? ".";
Console.WriteLine($"Ingesting from: {Path.GetFullPath(targetPath)}");

var extensions = new[] { ".cs", ".md", ".json", ".txt" };
var files = Directory
    .EnumerateFiles(targetPath, "*.*", SearchOption.AllDirectories)
    .Where(f => extensions.Contains(Path.GetExtension(f)))
    .ToList();

Console.WriteLine($"Found {files.Count} files.");

foreach (var file in files)
{
    var content = await File.ReadAllTextAsync(file);
    if (string.IsNullOrWhiteSpace(content)) continue;

    var chunks = chunker.Chunk(content, chunkSize: 500, overlap: 50);
    Console.WriteLine($"  {Path.GetFileName(file)} -> {chunks.Count} chunks");

    for (var i = 0; i < chunks.Count; i++)
    {
        var results = await embeddingService.GenerateAsync(chunks[i], cancellationToken: default);
        var embedding = new Vector(results.Vector.ToArray());
        var entity = new CodeChunkEntity
        {
            Id = Guid.NewGuid(),
            FilePath = Path.GetRelativePath(targetPath, file),
            Content = chunks[i],
            ChunkIndex = i,
            IngestedAt = DateTime.UtcNow,
            Embedding = new Vector(embedding.ToArray())
        };

        // Upsert — skip if same file+chunk already exists
        var exists = await db.CodeChunks
            .AnyAsync(c => c.FilePath == entity.FilePath && c.ChunkIndex == entity.ChunkIndex);

        if (!exists)
            db.CodeChunks.Add(entity);
        else
            Console.WriteLine($"    (skipping existing chunk {i})");
    }

    await db.SaveChangesAsync();
}

Console.WriteLine("Ingestion complete.");