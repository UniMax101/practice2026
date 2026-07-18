using System;
using System.Collections.Generic;
using System.Linq;

namespace task10;

public class PluginDependencyResolver
{
    /// <summary>
    /// </summary>
    public class PluginNode
    {
        public string Name { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string[] Dependencies { get; set; } = Array.Empty<string>();
        public Type PluginType { get; set; } = null!;
    }

    /// <summary>
    /// </summary>
    public static List<PluginNode> ResolveOrder(IEnumerable<PluginNode> plugins)
    {
        var pluginMap = plugins.ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
        var sorted = new List<PluginNode>();
        var visited = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        foreach (var plugin in pluginMap.Values)
        {
            Visit(plugin.Name, pluginMap, visited, sorted);
        }

        return sorted;
    }

    private static void Visit(
        string name, 
        Dictionary<string, PluginNode> pluginMap, 
        Dictionary<string, bool> visited, 
        List<PluginNode> sorted)
    {
        if (!pluginMap.ContainsKey(name)) return;
        if (visited.TryGetValue(name, out bool isCompleted))
        {
            if (!isCompleted)
            {
                throw new InvalidOperationException($"Обнаружена циклическая зависимость, включающая плагин: {name}");
            }
            return;
        }

        visited[name] = false;
        foreach (var dependency in pluginMap[name].Dependencies)
        {
            Visit(dependency, pluginMap, visited, sorted);
        }
        visited[name] = true;
        sorted.Add(pluginMap[name]);
    }
}