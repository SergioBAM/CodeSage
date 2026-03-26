using CodeSage.Api.Data;
using CodeSage.Api.Services;
using CodeSage.Core.Chunking;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Pgvector;
using Testcontainers.PostgreSql;
using CodeSage.Core.Embedding;

namespace CodeSage.Tests.Integration;

public class RagPipelineIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("pgvector/pgvector:pg16")        
        .WithDatabase("codesage_test")
        .WithUsername("codesage")
        .WithPassword("codesage")
        .Build();

    private AppDbContext _db = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString(), o => o.UseVector())
            .Options;

        _db = new AppDbContext(options);
        await _db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Ingest_ThenQuery_ReturnsRelevantChunk()
    {
        // arrange — seed a known chunk with a fake embedding
        var knownEmbedding = new float[768];
        knownEmbedding[0] = 1.0f; // distinct vector

        _db.CodeChunks.Add(new CodeChunkEntity
        {
            Id = Guid.NewGuid(),
            FilePath = "Services/RagQueryService.cs",
            Content = "public async Task FindRelevantChunksAsync() { }",
            ChunkIndex = 0,
            IngestedAt = DateTime.UtcNow,
            Embedding = new Vector(knownEmbedding)
        });

        await _db.SaveChangesAsync();

        // arrange — embedding service returns the same vector for our question
        var embeddingService = Substitute.For<IEmbeddingService>();
        embeddingService
            .EmbedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ReadOnlyMemory<float>(knownEmbedding));

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Rag:TopK"] = "1"
            })
            .Build();

        var ragService = new RagQueryService(_db, embeddingService, config);

        // act
        var results = await ragService.FindRelevantChunksAsync("how does chunk retrieval work?");

        // assert
        Assert.Single(results);
        Assert.Equal("Services/RagQueryService.cs", results[0].FilePath);
    }

    [Fact]
    public async Task Ingest_MultipleChunks_ReturnsTopK()
    {
        for (var i = 0; i < 10; i++)
        {
            var embedding = new float[768];
            embedding[i] = 1.0f;

            _db.CodeChunks.Add(new CodeChunkEntity
            {
                Id = Guid.NewGuid(),
                FilePath = $"File{i}.cs",
                Content = $"content {i}",
                ChunkIndex = 0,
                IngestedAt = DateTime.UtcNow,
                Embedding = new Vector(embedding)
            });
        }

        await _db.SaveChangesAsync();

        var embeddingService = Substitute.For<IEmbeddingService>();
        embeddingService
            .EmbedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ReadOnlyMemory<float>(new float[768]));

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Rag:TopK"] = "3"
            })
            .Build();

        var ragService = new RagQueryService(_db, embeddingService, config);

        var results = await ragService.FindRelevantChunksAsync("anything");

        Assert.Equal(3, results.Count);
    }
}