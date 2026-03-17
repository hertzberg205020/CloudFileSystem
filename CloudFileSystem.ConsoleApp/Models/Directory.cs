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
        ArgumentNullException.ThrowIfNull(component);
        if (component == this)
            throw new InvalidOperationException("不能將元件加為自己的子元件");
        if (IsAncestorOf(component))
            throw new InvalidOperationException("不能將祖先目錄加為子元件");
        if (_children.Any(c => c.Name == component.Name))
            throw new InvalidOperationException($"已存在同名子元件: {component.Name}");
        component.Parent = this;
        _children.Add(component);
    }

    /// <summary>
    /// 從此目錄移除指定的元件，並清除其 <see cref="FileSystemComponent.Parent"/> 參考。
    /// </summary>
    /// <param name="component">要移除的檔案或子目錄。</param>
    public void Remove(FileSystemComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);
        if (!_children.Remove(component))
            throw new InvalidOperationException("元件不在此目錄中");
        component.Parent = null;
    }

    /// <summary>
    /// 將指定的元件插入此目錄的指定位置。
    /// </summary>
    /// <param name="index">插入位置的索引。</param>
    /// <param name="component">要插入的檔案或子目錄。</param>
    public void Insert(int index, FileSystemComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _children.Count);
        component.Parent = this;
        _children.Insert(index, component);
    }

    /// <summary>
    /// 取得指定元件在此目錄中的索引位置。
    /// </summary>
    /// <param name="component">要查詢的元件。</param>
    /// <returns>元件的索引；若不存在則回傳 -1。</returns>
    public int IndexOf(FileSystemComponent component) => _children.IndexOf(component);

    /// <summary>
    /// 依指定的欄位與方向遞迴排序此目錄及所有子目錄的子元件。
    /// </summary>
    /// <param name="sortBy">排序依據。</param>
    /// <param name="sortOrder">排序方向。</param>
    public void Sort(SortBy sortBy, SortOrder sortOrder)
    {
        Comparison<FileSystemComponent> comparison = sortBy switch
        {
            SortBy.Name => (a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal),
            SortBy.Size => (a, b) => a.GetSize().CompareTo(b.GetSize()),
            SortBy.Extension => (a, b) =>
                string.Compare(
                    Path.GetExtension(a.Name),
                    Path.GetExtension(b.Name),
                    StringComparison.Ordinal
                ),
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy)),
        };

        _children.Sort(comparison);

        if (sortOrder == SortOrder.Desc)
            _children.Reverse();

        foreach (var child in _children.OfType<Directory>())
            child.Sort(sortBy, sortOrder);
    }

    /// <summary>
    /// 將子元件順序設定為指定的順序，用於 undo 還原排序。
    /// </summary>
    /// <param name="order">要還原的子元件順序。</param>
    public void SetChildrenOrder(IList<FileSystemComponent> order)
    {
        ArgumentNullException.ThrowIfNull(order);
        if (
            order.Count != _children.Count
            || !order.All(c => _children.Contains(c))
        )
            throw new ArgumentException("順序清單必須包含完全相同的子元件");
        _children.Clear();
        _children.AddRange(order);
    }

    private bool IsAncestorOf(FileSystemComponent component)
    {
        var current = Parent;
        while (current != null)
        {
            if (current == component)
                return true;
            current = current.Parent;
        }

        return false;
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

    /// <inheritdoc/>
    /// <remarks>遞迴深拷貝所有子元件，透過 <see cref="Add"/> 自動維護 <see cref="FileSystemComponent.Parent"/> 參照。</remarks>
    public override FileSystemComponent DeepCopy()
    {
        var copy = new Directory(Name);
        CopyTagsTo(copy);
        foreach (var child in _children)
            copy.Add(child.DeepCopy());
        return copy;
    }
}
