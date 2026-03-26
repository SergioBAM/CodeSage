using CodeSage.Core.Chunking;

namespace CodeSage.Tests.Chunking;

public class RoslynCodeChunkerTests
{
    private readonly RoslynCodeChunker _chunker = new();

    [Fact]
    public void Chunk_SingleMethod_ProducesOneChunk()
    {
        var source = """
            namespace MyApp;
            public class MyService
            {
                public void DoSomething() { }
            }
            """;

        var chunks = _chunker.ChunkFile("MyService.cs", source);

        Assert.Single(chunks);
    }

    [Fact]
    public void Chunk_TwoMethods_ProducesTwoChunks()
    {
        var source = """
            namespace MyApp;
            public class MyService
            {
                public void MethodOne() { }
                public void MethodTwo() { }
            }
            """;

        var chunks = _chunker.ChunkFile("MyService.cs", source);

        Assert.Equal(2, chunks.Count);
    }

    [Fact]
    public void Chunk_IncludesFilePathInOutput()
    {
        var source = """
            namespace MyApp;
            public class MyService
            {
                public void DoSomething() { }
            }
            """;

        var chunks = _chunker.ChunkFile("Services/MyService.cs", source);

        Assert.Contains("Services/MyService.cs", chunks[0]);
    }

    [Fact]
    public void Chunk_IncludesClassNameInOutput()
    {
        var source = """
            namespace MyApp;
            public class MyService
            {
                public void DoSomething() { }
            }
            """;

        var chunks = _chunker.ChunkFile("MyService.cs", source);

        Assert.Contains("MyService", chunks[0]);
    }

    [Fact]
    public void Chunk_IncludesMemberNameInOutput()
    {
        var source = """
            namespace MyApp;
            public class MyService
            {
                public void DoSomething() { }
            }
            """;

        var chunks = _chunker.ChunkFile("MyService.cs", source);

        Assert.Contains("DoSomething", chunks[0]);
    }

    [Fact]
    public void Chunk_EmptyFile_ReturnsFallbackChunk()
    {
        var source = "// just a comment, no members";

        var chunks = _chunker.ChunkFile("Empty.cs", source);

        // falls back to whole file as single chunk
        Assert.Single(chunks);
    }

    [Fact]
    public void Chunk_MultipleClasses_ProducesChunkPerMember()
    {
        var source = """
            namespace MyApp;
            public class ServiceA
            {
                public void MethodA() { }
            }
            public class ServiceB
            {
                public void MethodB() { }
                public void MethodC() { }
            }
            """;

        var chunks = _chunker.ChunkFile("Combined.cs", source);

        Assert.Equal(3, chunks.Count);
    }
}