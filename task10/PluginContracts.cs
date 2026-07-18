using System;

namespace task10;

/// <summary>
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class PluginLoadAttribute : Attribute
{
    public string[] Dependencies { get; }

    public PluginLoadAttribute(params string[] dependencies)
    {
        Dependencies = dependencies ?? Array.Empty<string>();
    }
}

/// <summary>
/// </summary>
public interface ICommand
{
    void Execute();
}