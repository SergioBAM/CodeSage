using System.Dynamic;

namespace CodeSage.Core.Models;

public sealed record CodeChunk
{
    public Guid Id {get; init;} = Guid.CreateVersion7();
    public required string FilePath {get; init;}
    public required string Content {get; init;}
    public required int ChunkIndex {get; init;}
    public DateTime IngestedAt {get;init;} = DateTime.UtcNow;
}