using System;
using System.Collections.Generic;
using Xunit;
using task10;

namespace task10tests;

public class PluginDependencyTests
{
    [Fact]
    public void ResolveOrder_ValidDependencies_ReturnsCorrectOrder()
    {
        var plugins = new List<PluginDependencyResolver.PluginNode>
        {
            new() { Name = "PluginC", Dependencies = new[] { "PluginB" } },
            new() { Name = "PluginB", Dependencies = new[] { "PluginA" } },
            new() { Name = "PluginA", Dependencies = Array.Empty<string>() }
        };

        var result = PluginDependencyResolver.ResolveOrder(plugins);
        Assert.Equal(3, result.Count);
        Assert.Equal("PluginA", result[0].Name);
        Assert.Equal("PluginB", result[1].Name);
        Assert.Equal("PluginC", result[2].Name);
    }

    [Fact]
    public void ResolveOrder_CyclicDependencies_ThrowsInvalidOperationException()
    {
        var plugins = new List<PluginDependencyResolver.PluginNode>
        {
            new() { Name = "PluginA", Dependencies = new[] { "PluginB" } },
            new() { Name = "PluginB", Dependencies = new[] { "PluginA" } }
        };

        Assert.Throws<InvalidOperationException>(() => PluginDependencyResolver.ResolveOrder(plugins));
    }
}