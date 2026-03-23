using CodeSage.Api.Data;
using CodeSage.Core.Embedding;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

public sealed class RagQueryService
{
    private readonly AppDbContext _db;
    private readonly IEmbeddingService _embedding;
    private readonly int _topK;
    public RagQueryService(
        AppDbContext db,
        IEmbeddingService embedding,
        IConfiguration config)
    {
        _db = db;
        _embedding = embedding;
        _topK = config.GetValue<int>("Rag:TopK", 5);
    }

    public async Task<IReadOnlyList<CodeChunkEntity>> FindRelevantChunksAsync(
        string question,        
        CancellationToken cancellationToken = default)
    {
        var questionEmbedding = await _embedding.EmbedAsync(question, cancellationToken);
        var vector = new Pgvector.Vector(questionEmbedding.ToArray());

        return await _db.CodeChunks
            .OrderBy(c => c.Embedding!.L2Distance(vector))
            .Take(_topK)
            .ToListAsync(cancellationToken);
    }
}