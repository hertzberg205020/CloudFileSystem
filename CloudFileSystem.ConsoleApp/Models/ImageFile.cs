using CloudFileSystem.ConsoleApp.Visitors;

namespace CloudFileSystem.ConsoleApp.Models;

public class ImageFile : File
{
    public ImageFile(string name, long size, DateTime createdAt, int width, int height)
        : base(name, size, createdAt)
    {
        Width = width;
        Height = height;
    }

    public int Width { get; }
    public int Height { get; }

    public override void Accept(IFileSystemVisitor visitor)
    {
        visitor.Visit(this);
    }
}
