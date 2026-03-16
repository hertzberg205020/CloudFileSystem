using CloudFileSystem.ConsoleApp.Visitors;

namespace CloudFileSystem.ConsoleApp.Models;

public class TextFile : File
{
    public TextFile(string name, long size, DateTime createdAt, string encoding)
        : base(name, size, createdAt)
    {
        Encoding = encoding;
    }

    public string Encoding { get; }

    public override void Accept(IFileSystemVisitor visitor)
    {
        visitor.Visit(this);
    }
}
