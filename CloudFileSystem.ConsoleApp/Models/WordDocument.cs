using CloudFileSystem.ConsoleApp.Visitors;

namespace CloudFileSystem.ConsoleApp.Models;

public class WordDocument : File
{
    public WordDocument(string name, long size, DateTime createdAt, int pageCount)
        : base(name, size, createdAt)
    {
        PageCount = pageCount;
    }

    public int PageCount { get; }

    public override void Accept(IFileSystemVisitor visitor)
    {
        visitor.Visit(this);
    }
}
