using NetArchTest.Rules;

namespace CodeSage.Tests;

public class ArchitectureTests
{
    [Fact]
    public void Core_ShouldNot_DependOn_Api()
    {
        var result = Types.InAssembly(typeof(CodeSage.Core.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("CodeSage.Api")
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Core must not reference the Api layer — dependency should flow inward.");
    }
}