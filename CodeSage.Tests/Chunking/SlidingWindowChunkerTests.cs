using CodeSage.Core.Chunking;

namespace CodeSage.Tests.Chunking;

public class SlidingWindowChunkerTests
{
    private readonly ITextChunker _chunker = new SlidingWindowChunker();

    [Fact]
    public void Chunk_SortText_ReturnsSingleChunk()
    {
        var result = _chunker.Chunk("hello world", chunkSize: 500);
        Assert.Single(result);
        Assert.Equal("hello world", result[0]);
    }

    [Fact]
    public void Chunk_LongText_ReturnsMultipleChunks()
    {
        var text = new string('a', 1000);
        var result = _chunker.Chunk(text, chunkSize: 500, overlap: 50);
        
        Assert.True(result.Count > 1);
    }

    [Fact]
    public void Chunk_OverlapIsApplied_ChunksShareContent()
    {
        var text = new string('a', 100);
        var result = _chunker.Chunk(text, chunkSize: 50, overlap: 20);
        
        var endOfFirst = result[0][^20..];
        var startOfSecond = result[1][..20];
        Assert.Equal(endOfFirst, startOfSecond);
    }

    [Fact]
    public void Chunk_EmptyList_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => _chunker.Chunk(""));
    }

    [Fact]
    public void Chunk_OverlapLargerThanChunkSize_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            _chunker.Chunk("foobar", chunkSize:10, overlap: 10));
    }
}