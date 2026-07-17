using System;
using System.IO;
using System.Linq;

public class DirectorySizeCommand : ICommand
{
    private readonly string _directory;

    public DirectorySizeCommand(string directory)
    {
        if (directory == null) throw new ArgumentNullException(nameof(directory));
        _directory = directory;
    }

    public void Execute()
    {
        var size = Directory.GetFiles(_directory, "*", SearchOption.AllDirectories)
            .Sum(f => new FileInfo(f).Length);
        Console.WriteLine($"Размер каталога '{_directory}': {size} байт");
    }
}
