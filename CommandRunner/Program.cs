using System;
using System.IO;
using System.Linq;
using System.Reflection;

var assemblyPath = Path.Combine(AppContext.BaseDirectory, "FileSystemCommands.dll");
var assembly = Assembly.LoadFrom(assemblyPath);

var commandTypes = assembly.GetTypes()
    .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsInterface);

foreach (var type in commandTypes)
{
    Console.WriteLine($"Найден класс: {type.Name}");
}

var testDir = Path.Combine(Path.GetTempPath(), "TestDir");
Directory.CreateDirectory(testDir);
File.WriteAllText(Path.Combine(testDir, "file1.txt"), "Hello");
File.WriteAllText(Path.Combine(testDir, "file2.log"), "Log");

ICommand sizeCommand = new DirectorySizeCommand(testDir);
sizeCommand.Execute();

ICommand findCommand = new FindFilesCommand(testDir, "*.txt");
findCommand.Execute();

Directory.Delete(testDir, true);
