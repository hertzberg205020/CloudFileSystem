namespace CloudFileSystem.ConsoleApp.Models;

/// <summary>
/// 檔案的抽象基底類別，為 Composite Pattern 中的 Leaf 角色。
/// </summary>
/// <remarks>
/// 所有具體檔案類型（<see cref="WordDocument"/>、<see cref="ImageFile"/>、<see cref="TextFile"/>）皆繼承此類別。
/// </remarks>
public abstract class File : FileSystemComponent
{
    /// <summary>
    /// 初始化 <see cref="File"/> 的新執行個體。
    /// </summary>
    /// <param name="name">檔案名稱，包含副檔名。</param>
    /// <param name="size">檔案大小（單位：Bytes）。</param>
    /// <param name="createdAt">檔案建立時間。</param>
    protected File(string name, long size, DateTime createdAt)
        : base(name)
    {
        Size = size;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// 取得檔案大小（單位：Bytes）。
    /// </summary>
    public long Size { get; }

    /// <summary>
    /// 取得檔案建立時間。
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <inheritdoc/>
    public override long GetSize()
    {
        return Size;
    }

    /// <summary>
    /// 將 Bytes 數值格式化為人類可讀的大小字串。
    /// </summary>
    /// <param name="bytes">要格式化的位元組數。</param>
    /// <returns>格式化後的字串，例如 <c>"500KB"</c>、<c>"2MB"</c> 或 <c>"500B"</c>。</returns>
    public static string FormatSize(long bytes)
    {
        if (bytes >= 1024 * 1024 && bytes % (1024 * 1024) == 0)
            return $"{bytes / (1024 * 1024)}MB";
        if (bytes >= 1024 && bytes % 1024 == 0)
            return $"{bytes / 1024}KB";
        return $"{bytes}B";
    }
}
