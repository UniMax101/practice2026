using System;
using System.IO;
using Xunit;

public class FileSystemCommandsTests
{
    [Fact]
    public void DirectorySizeCommand_ShouldCalculateSize()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "TestDir_Size");
        Directory.CreateDirectory(testDir);
        File.WriteAllText(Path.Combine(testDir, "test1.txt"), "Hello");
        File.WriteAllText(Path.Combine(testDir, "test2.txt"), "World");

        var command = new DirectorySizeCommand(testDir);
        command.Execute();

        Directory.Delete(testDir, true);
    }

    [Fact]
    public void DirectorySizeCommand_ShouldThrowOnNullDirectory()
    {
        Assert.Throws<ArgumentNullException>(() => new DirectorySizeCommand(null));
    }

    [Fact]
    public void FindFilesCommand_ShouldFindMatchingFiles()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "TestDir_Find");
        Directory.CreateDirectory(testDir);
        File.WriteAllText(Path.Combine(testDir, "file1.txt"), "Text");
        File.WriteAllText(Path.Combine(testDir, "file2.log"), "Log");

        var command = new FindFilesCommand(testDir, "*.txt");
        command.Execute();

        Directory.Delete(testDir, true);
    }

    [Fact]
    public void FindFilesCommand_ShouldThrowOnNullDirectory()
    {
        Assert.Throws<ArgumentNullException>(() => new FindFilesCommand(null, "*.txt"));
    }

    [Fact]
    public void FindFilesCommand_ShouldThrowOnNullMask()
    {
        Assert.Throws<ArgumentNullException>(() => new FindFilesCommand("/tmp", null));
    }
}
