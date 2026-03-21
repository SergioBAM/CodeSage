namespace CodeSage.Core.Chunking;

public sealed class SlidingWindowChunker : ITextChunker
{
    public IReadOnlyList<string> Chunk(string text, int chunkSize = 500, int overlap = 50)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentOutOfRangeException.ThrowIfLessThan(chunkSize, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(overlap, chunkSize);

        var chunks = new List<string>();
        var start = 0;

        while (start < text.Length)
        {
            var length = Math.Min(chunkSize, text.Length - start);
            chunks.Add(text.Substring(start, length));
            start += chunkSize - overlap;
        }

        return chunks.AsReadOnly();
    }
}