using CodeSage.Api.Data;
using CodeSage.Core.Embedding;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

public sealed class RagQueryService
{
    private readonly AppDbContext _db;
    private readonly IEmbeddingService _embedding;

    public RagQueryService(AppDbContext db, IEmbeddingService embedding)
    {
        _db = db;
        _embedding = embedding;
    }

    public async Task<IReadOnlyList<CodeChunkEntity>> FindRelevantChunksAsync(
        string question,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        var questionEmbedding = await _embedding.EmbedAsync(question, cancellationToken);
        var vector = new Pgvector.Vector(questionEmbedding.ToArray());

        return await _db.CodeChunks
            .OrderBy(c => c.Embedding!.L2Distance(vector))
            .Take(topK)
            .ToListAsync(cancellationToken);
    }
}