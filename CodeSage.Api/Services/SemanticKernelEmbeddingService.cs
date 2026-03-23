using CodeSage.Core.Embedding;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Embeddings;

public sealed class SemanticKernelEmbeddingService : IEmbeddingService
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;

    public SemanticKernelEmbeddingService(IEmbeddingGenerator<string, Embedding<float>> generator)
    {
        _generator = generator;
    }

    public async Task<ReadOnlyMemory<float>> EmbedAsync(string text, CancellationToken token = default)
    {
        var results = await _generator.GenerateAsync(text, cancellationToken: token);
        return results.Vector;
    }
}