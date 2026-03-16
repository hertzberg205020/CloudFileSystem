using CloudFileSystem.ConsoleApp.Visitors;

namespace CloudFileSystem.ConsoleApp.Models;

/// <summary>
/// 代表檔案系統中的目錄，為 Composite Pattern 中的 Composite 角色。
/// </summary>
/// <remarks>
/// 目錄可包含任意數量的 <see cref="FileSystemComponent"/>（子目錄或檔案），支援無限層級嵌套。
/// </remarks>
public class Directory : FileSystemComponent
{
    private readonly List<FileSystemComponent> _children = [];

    /// <summary>
    /// 初始化 <see cref="Directory"/> 的新執行個體。
    /// </summary>
    /// <param name="name">目錄名稱。</param>
    public Directory(string name)
        : base(name) { }

    /// <summary>
    /// 取得此目錄下的所有子元件。
    /// </summary>
    /// <value>子目錄與檔案的唯讀集合。</value>
    public IReadOnlyList<FileSystemComponent> Children => _children;

    /// <summary>
    /// 將指定的元件加入此目錄，並設定其 <see cref="FileSystemComponent.Parent"/> 為此目錄。
    /// </summary>
    /// <param name="component">要加入的檔案或子目錄。</param>
    public void Add(FileSystemComponent component)
    {
        component.Parent = this;
        _children.Add(component);
    }

    /// <summary>
    /// 從此目錄移除指定的元件，並清除其 <see cref="FileSystemComponent.Parent"/> 參考。
    /// </summary>
    /// <param name="component">要移除的檔案或子目錄。</param>
    public void Remove(FileSystemComponent component)
    {
        component.Parent = null;
        _children.Remove(component);
    }

    /// <inheritdoc/>
    /// <remarks>遞迴加總所有子元件的大小。</remarks>
    public override long GetSize()
    {
        return _children.Sum(c => c.GetSize());
    }

    /// <inheritdoc/>
    public override void Accept(IFileSystemVisitor visitor)
    {
        visitor.Visit(this);
    }
}
