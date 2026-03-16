using CloudFileSystem.ConsoleApp.Visitors;

namespace CloudFileSystem.ConsoleApp.Models;

public class Directory : FileSystemComponent
{
    private readonly List<FileSystemComponent> _children = [];

    public Directory(string name)
        : base(name) { }

    public IReadOnlyList<FileSystemComponent> Children => _children;

    public void Add(FileSystemComponent component)
    {
        component.Parent = this;
        _children.Add(component);
    }

    public void Remove(FileSystemComponent component)
    {
        component.Parent = null;
        _children.Remove(component);
    }

    public override long GetSize()
    {
        return _children.Sum(c => c.GetSize());
    }

    public override void Accept(IFileSystemVisitor visitor)
    {
        visitor.Visit(this);
    }
}
