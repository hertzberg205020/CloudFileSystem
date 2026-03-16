using CloudFileSystem.ConsoleApp.Models;

namespace CloudFileSystem.ConsoleApp.Visitors;

/// <summary>
/// 檔案系統 Visitor 介面，定義走訪各類元件的方法。
/// </summary>
/// <remarks>
/// 透過 <see cref="FileSystemComponent.Accept"/> 實現雙重分派，
/// 讓每種元件呼叫對應的 <c>Visit</c> 多載方法。
/// </remarks>
public interface IFileSystemVisitor
{
    /// <summary>
    /// 走訪指定的目錄節點。
    /// </summary>
    /// <param name="directory">要走訪的目錄。</param>
    void Visit(Directory directory);

    /// <summary>
    /// 走訪指定的 Word 文件節點。
    /// </summary>
    /// <param name="file">要走訪的 Word 文件。</param>
    void Visit(WordDocument file);

    /// <summary>
    /// 走訪指定的圖片檔案節點。
    /// </summary>
    /// <param name="file">要走訪的圖片檔案。</param>
    void Visit(ImageFile file);

    /// <summary>
    /// 走訪指定的純文字檔案節點。
    /// </summary>
    /// <param name="file">要走訪的純文字檔案。</param>
    void Visit(TextFile file);
}
