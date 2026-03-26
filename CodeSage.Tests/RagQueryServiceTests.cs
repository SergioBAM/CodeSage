using CodeSage.Api.Data;
using CodeSage.Api.Services;
using CodeSage.Core.Embedding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace CodeSage.Tests.Services;

public class RagQueryServiceTests
{
    [Fact]
    public void RagQueryService_ReadsTopK_FromConfiguration()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Rag:TopK"] = "7"
            })
            .Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("test-topk")
            .Options;

        var db = new AppDbContext(options);
        var embedding = Substitute.For<IEmbeddingService>();

        var service = new RagQueryService(db, embedding, config);

        Assert.Equal(7, service.TopK);
    }

    [Fact]
    public void RagQueryService_DefaultsTopK_ToFive_WhenNotConfigured()
    {
        var config = new ConfigurationBuilder()
            .Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("test-topk-default")
            .Options;

        var db = new AppDbContext(options);
        var embedding = Substitute.For<IEmbeddingService>();

        var service = new RagQueryService(db, embedding, config);

        Assert.Equal(5, service.TopK);
    }
}