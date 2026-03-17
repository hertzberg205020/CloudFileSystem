using CloudFileSystem.ConsoleApp.Visitors;

namespace CloudFileSystem.ConsoleApp.Models;

/// <summary>
/// 代表 Word 文件檔案（<c>.docx</c>），具有頁數屬性。
/// </summary>
public class WordDocument : File
{
    /// <summary>
    /// 初始化 <see cref="WordDocument"/> 的新執行個體。
    /// </summary>
    /// <param name="name">檔案名稱，例如 <c>"需求規格書.docx"</c>。</param>
    /// <param name="size">檔案大小（單位：Bytes）。</param>
    /// <param name="createdAt">檔案建立時間。</param>
    /// <param name="pageCount">文件頁數。</param>
    public WordDocument(string name, long size, DateTime createdAt, int pageCount)
        : base(name, size, createdAt)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(pageCount);
        PageCount = pageCount;
    }

    /// <summary>
    /// 取得文件的頁數。
    /// </summary>
    public int PageCount { get; }

    /// <inheritdoc/>
    public override void Accept(IFileSystemVisitor visitor)
    {
        visitor.Visit(this);
    }

    /// <inheritdoc/>
    public override FileSystemComponent DeepCopy()
    {
        var copy = new WordDocument(Name, Size, CreatedAt, PageCount);
        CopyTagsTo(copy);
        return copy;
    }
}
