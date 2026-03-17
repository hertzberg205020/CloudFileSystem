using CloudFileSystem.ConsoleApp.Visitors;

namespace CloudFileSystem.ConsoleApp.Models;

/// <summary>
/// 代表純文字檔案（<c>.txt</c>），具有編碼屬性。
/// </summary>
public class TextFile : File
{
    /// <summary>
    /// 初始化 <see cref="TextFile"/> 的新執行個體。
    /// </summary>
    /// <param name="name">檔案名稱，例如 <c>"待辦清單.txt"</c>。</param>
    /// <param name="size">檔案大小（單位：Bytes）。</param>
    /// <param name="createdAt">檔案建立時間。</param>
    /// <param name="encoding">文字編碼格式，例如 <c>"UTF-8"</c> 或 <c>"ASCII"</c>。</param>
    public TextFile(string name, long size, DateTime createdAt, string encoding)
        : base(name, size, createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(encoding);
        Encoding = encoding;
    }

    /// <summary>
    /// 取得此檔案的文字編碼格式。
    /// </summary>
    /// <value>編碼名稱，例如 <c>"UTF-8"</c> 或 <c>"ASCII"</c>。</value>
    public string Encoding { get; }

    /// <inheritdoc/>
    public override void Accept(IFileSystemVisitor visitor)
    {
        visitor.Visit(this);
    }

    /// <inheritdoc/>
    public override FileSystemComponent DeepCopy()
    {
        var copy = new TextFile(Name, Size, CreatedAt, Encoding);
        CopyTagsTo(copy);
        return copy;
    }
}
