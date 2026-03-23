namespace CodeSage.Core.Embedding;

public interface IEmbeddingService
{
    Task<ReadOnlyMemory<float>> EmbedAsync(string text, CancellationToken token = default);
}
