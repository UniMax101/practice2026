using System;
using System.IO;

namespace task10;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Системный загрузчик динамических плагинов (.NET 10) ===");

        string pluginsPath = args.Length > 0 
            ? args[0] 
            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");

        Console.WriteLine($"Целевая директория: {pluginsPath}");

        if (!Directory.Exists(pluginsPath))
        {
            Directory.CreateDirectory(pluginsPath);
            Console.WriteLine("Папка 'plugins' была пуста и автоматически создана.");
            Console.WriteLine("Пожалуйста, поместите туда скомпилированные .dll файлы плагинов.");
            return;
        }

        try
        {
            var loader = new PluginLoader(pluginsPath);
            loader.LoadAndExecutePlugins();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Критический сбой приложения: {ex.Message}");
            Console.ResetColor();
        }

        Console.WriteLine("\nРабота программы завершена.");
    }
}