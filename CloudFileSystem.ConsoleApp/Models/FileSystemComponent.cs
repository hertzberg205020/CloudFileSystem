using CloudFileSystem.ConsoleApp.Visitors;

namespace CloudFileSystem.ConsoleApp.Models;

public abstract class FileSystemComponent
{
    protected FileSystemComponent(string name)
    {
        Name = name;
    }

    public string Name { get; }
    public Directory? Parent { get; internal set; }

    public abstract long GetSize();

    public abstract void Accept(IFileSystemVisitor visitor);

    public string GetPath()
    {
        if (Parent == null)
            return Name;
        return Parent.GetPath() + "/" + Name;
    }
}
