namespace CloudFileSystem.ConsoleApp.Models;

public abstract class File : FileSystemComponent
{
    protected File(string name, long size, DateTime createdAt)
        : base(name)
    {
        Size = size;
        CreatedAt = createdAt;
    }

    public long Size { get; }
    public DateTime CreatedAt { get; }

    public override long GetSize()
    {
        return Size;
    }

    public static string FormatSize(long bytes)
    {
        if (bytes >= 1024 * 1024 && bytes % (1024 * 1024) == 0)
            return $"{bytes / (1024 * 1024)}MB";
        if (bytes >= 1024 && bytes % 1024 == 0)
            return $"{bytes / 1024}KB";
        return $"{bytes}B";
    }
}
