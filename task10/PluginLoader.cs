using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace task10;

public class PluginLoader
{
    private readonly string _pluginsDirectory;
    private readonly AssemblyLoadContext _loadContext;

    public PluginLoader(string pluginsDirectory)
    {
        _pluginsDirectory = pluginsDirectory;
        _loadContext = new AssemblyLoadContext("PluginLoadingContext", isCollectible: true);
    }

    /// <summary>
    ///</summary>
    public void LoadAndExecutePlugins()
    {
        if (!Directory.Exists(_pluginsDirectory))
        {
            Console.WriteLine($"Папка с плагинами не найдена: {_pluginsDirectory}");
            return;
        }

        var dllFiles = Directory.GetFiles(_pluginsDirectory, "*.dll");
        var rawNodes = new List<PluginDependencyResolver.PluginNode>();

        foreach (var file in dllFiles)
        {
            try
            {
                var assembly = _loadContext.LoadFromAssemblyPath(Path.GetFullPath(file));
                var assemblyName = assembly.GetName().Name ?? Path.GetFileNameWithoutExtension(file);

                foreach (var type in assembly.GetTypes())
                {
                    var attribute = type.GetCustomAttribute<PluginLoadAttribute>();
                    if (attribute != null)
                    {
                        rawNodes.Add(new PluginDependencyResolver.PluginNode
                        {
                            Name = assemblyName,
                            FilePath = file,
                            Dependencies = attribute.Dependencies,
                            PluginType = type
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось проанализировать файл {Path.GetFileName(file)}: {ex.Message}");
            }
        }
        List<PluginDependencyResolver.PluginNode> executionOrder;
        try
        {
            executionOrder = PluginDependencyResolver.ResolveOrder(rawNodes);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Ошибка при разрешении зависимостей: {ex.Message}");
            return;
        }
        Console.WriteLine("\n--- Запуск плагинов в порядке зависимостей ---");
        foreach (var node in executionOrder)
        {
            try
            {
                Console.WriteLine($"\n[Запуск] Плагин: {node.Name} (Тип: {node.PluginType.Name})");
                object? pluginInstance = Activator.CreateInstance(node.PluginType);

                if (pluginInstance == null)
                {
                    Console.WriteLine($"Ошибка: Не удалось создать экземпляр для {node.PluginType.Name}");
                    continue;
                }
                if (pluginInstance is ICommand command)
                {
                    command.Execute();
                }
                else
                {
                    var method = node.PluginType.GetMethod("Execute", BindingFlags.Public | BindingFlags.Instance);
                    if (method != null && method.GetParameters().Length == 0)
                    {
                        method.Invoke(pluginInstance, null);
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка: У класса {node.PluginType.Name} отсутствует подходящий метод Execute().");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Исключение при выполнении плагина {node.Name}: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
}