namespace CodeSage.Core.Chunking;

public interface ITextChunker
{
    IReadOnlyList<string> Chunk(string text, int chunkSize=500, int overlap = 50);
}