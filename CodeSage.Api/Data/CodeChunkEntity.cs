using Pgvector;

namespace CodeSage.Api.Data;

public sealed class CodeChunkEntity
{
    public Guid Id {get;set;}
    public string FilePath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public DateTime IngestedAt { get; set; }
    public Vector? Embedding { get; set; }
}