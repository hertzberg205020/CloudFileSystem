using CloudFileSystem.ConsoleApp.Visitors;

namespace CloudFileSystem.ConsoleApp.Models;

/// <summary>
/// 代表圖片檔案（如 <c>.png</c>、<c>.jpg</c>），具有解析度屬性。
/// </summary>
public class ImageFile : File
{
    /// <summary>
    /// 初始化 <see cref="ImageFile"/> 的新執行個體。
    /// </summary>
    /// <param name="name">檔案名稱，例如 <c>"系統架構圖.png"</c>。</param>
    /// <param name="size">檔案大小（單位：Bytes）。</param>
    /// <param name="createdAt">檔案建立時間。</param>
    /// <param name="width">圖片寬度（單位：像素）。</param>
    /// <param name="height">圖片高度（單位：像素）。</param>
    public ImageFile(string name, long size, DateTime createdAt, int width, int height)
        : base(name, size, createdAt)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// 取得圖片寬度（單位：像素）。
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// 取得圖片高度（單位：像素）。
    /// </summary>
    public int Height { get; }

    /// <inheritdoc/>
    public override void Accept(IFileSystemVisitor visitor)
    {
        visitor.Visit(this);
    }
}
