using System;
using System.IO;

public class FindFilesCommand : ICommand
{
    private readonly string _directory;
    private readonly string _mask;

    public FindFilesCommand(string directory, string mask)
    {
        if (directory == null) throw new ArgumentNullException(nameof(directory));
        if (mask == null) throw new ArgumentNullException(nameof(mask));
        _directory = directory;
        _mask = mask;
    }

    public void Execute()
    {
        var files = Directory.GetFiles(_directory, _mask, SearchOption.AllDirectories);
        Console.WriteLine($"Найдено файлов по маске '{_mask}' в '{_directory}': {files.Length}");
        foreach (var file in files)
            Console.WriteLine($"  {file}");
    }
}
