using NetArchTest.Rules;
using System.Reflection;

namespace CodeSage.Tests;

public class ArchitectureTests
{
    private static readonly Assembly CoreAssembly =
        typeof(CodeSage.Core.AssemblyMarker).Assembly;

    private static readonly Assembly ApiAssembly =
        typeof(Program).Assembly;

    [Fact]
    public void Core_ShouldNot_DependOn_Api()
    {
        var result = Types.InAssembly(CoreAssembly)
            .ShouldNot()
            .HaveDependencyOn("CodeSage.Api")
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Core must not reference the Api layer.");
    }

    [Fact]
    public void Core_ShouldNot_DependOn_Ingestion()
    {
        var result = Types.InAssembly(CoreAssembly)
            .ShouldNot()
            .HaveDependencyOn("CodeSage.Ingestion")
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Core must not reference the Ingestion layer.");
    }

    [Fact]
    public void Services_ShouldLiveIn_ServicesNamespace()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That()
            .HaveNameEndingWith("Service")
            .Should()
            .ResideInNamespace("CodeSage.Api.Services")
            .GetResult();

        Assert.True(result.IsSuccessful,
            "All service classes must live in CodeSage.Api.Services.");
    }

    [Fact]
    public void Interfaces_ShouldLiveIn_Core()
    {
        var result = Types.InAssembly(CoreAssembly)
            .That()
            .AreInterfaces()
            .Should()
            .ResideInNamespaceStartingWith("CodeSage.Core")
            .GetResult();

        Assert.True(result.IsSuccessful,
            "All interfaces must be defined in CodeSage.Core.");
    }

    [Fact]
    public void ApiControllers_ShouldNot_Exist()
    {
        // we use minimal API — no MVC controllers should ever appear
        var result = Types.InAssembly(ApiAssembly)
            .ShouldNot()
            .HaveNameEndingWith("Controller")
            .GetResult();

        Assert.True(result.IsSuccessful,
            "This project uses Minimal API — no MVC controllers allowed.");
    }
}