using CloudFileSystem.ConsoleApp.Models;

namespace CloudFileSystem.ConsoleApp.Visitors;

public class SizeCalculatorVisitor : IFileSystemVisitor
{
    private readonly IConsole _console;

    public long TotalSize { get; private set; }

    public SizeCalculatorVisitor(IConsole console)
    {
        _console = console;
    }

    public void Visit(Directory directory)
    {
        _console.WriteLine($"Visiting: {directory.GetPath()}");
        foreach (var child in directory.Children)
            child.Accept(this);
    }

    public void Visit(WordDocument file)
    {
        _console.WriteLine($"Visiting: {file.GetPath()}");
        TotalSize += file.Size;
    }

    public void Visit(ImageFile file)
    {
        _console.WriteLine($"Visiting: {file.GetPath()}");
        TotalSize += file.Size;
    }

    public void Visit(TextFile file)
    {
        _console.WriteLine($"Visiting: {file.GetPath()}");
        TotalSize += file.Size;
    }
}
