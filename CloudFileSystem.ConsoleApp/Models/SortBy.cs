namespace CloudFileSystem.ConsoleApp.Models;

/// <summary>
/// 目錄排序的依據欄位。
/// </summary>
public enum SortBy
{
    /// <summary>依名稱排序。</summary>
    Name,

    /// <summary>依大小排序。</summary>
    Size,

    /// <summary>依副檔名排序。</summary>
    Extension,
}
