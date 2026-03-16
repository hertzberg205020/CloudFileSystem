using CloudFileSystem.ConsoleApp.Models;

namespace CloudFileSystem.ConsoleApp.Visitors;

public class SearchByExtensionVisitor : IFileSystemVisitor
{
    private readonly string _targetExtension;
    private readonly IConsole _console;

    public List<string> Results { get; } = [];

    public SearchByExtensionVisitor(string extension, IConsole console)
    {
        _targetExtension = extension.StartsWith('.') ? extension : "." + extension;
        _console = console;
    }

    public void Visit(Directory directory)
    {
        _console.WriteLine($"Visiting: {directory.GetPath()}");
        foreach (var child in directory.Children)
            child.Accept(this);
    }

    public void Visit(WordDocument file) => VisitFile(file);

    public void Visit(ImageFile file) => VisitFile(file);

    public void Visit(TextFile file) => VisitFile(file);

    private void VisitFile(FileSystemComponent file)
    {
        _console.WriteLine($"Visiting: {file.GetPath()}");
        if (file.Name.EndsWith(_targetExtension, StringComparison.OrdinalIgnoreCase))
            Results.Add(file.GetPath());
    }
}
