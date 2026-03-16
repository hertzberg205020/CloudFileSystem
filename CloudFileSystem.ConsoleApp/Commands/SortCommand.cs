using CloudFileSystem.ConsoleApp.Models;

namespace CloudFileSystem.ConsoleApp.Commands;

/// <summary>
/// 遞迴排序目錄子元件的 Command，Undo 時還原整棵子樹的原始順序。
/// </summary>
public class SortCommand : ICommand
{
    private readonly Directory _directory;
    private readonly SortBy _sortBy;
    private readonly SortOrder _sortOrder;
    private readonly Dictionary<Directory, List<FileSystemComponent>> _originalOrders;

    /// <summary>
    /// 初始化 <see cref="SortCommand"/> 的新執行個體。
    /// </summary>
    /// <param name="directory">要排序的目錄。</param>
    /// <param name="sortBy">排序依據。</param>
    /// <param name="sortOrder">排序方向。</param>
    public SortCommand(Directory directory, SortBy sortBy, SortOrder sortOrder)
    {
        _directory = directory;
        _sortBy = sortBy;
        _sortOrder = sortOrder;
        _originalOrders = SnapshotOrder(directory);
    }

    /// <inheritdoc/>
    public string Description => $"Sort by {_sortBy} {_sortOrder}";

    /// <inheritdoc/>
    public void Execute() => _directory.Sort(_sortBy, _sortOrder);

    /// <inheritdoc/>
    public void Undo()
    {
        foreach (var (dir, order) in _originalOrders)
            dir.SetChildrenOrder(order);
    }

    private static Dictionary<Directory, List<FileSystemComponent>> SnapshotOrder(
        Directory directory)
    {
        var snapshot = new Dictionary<Directory, List<FileSystemComponent>>
        {
            [directory] = directory.Children.ToList(),
        };

        foreach (var child in directory.Children.OfType<Directory>())
        {
            foreach (var entry in SnapshotOrder(child))
                snapshot[entry.Key] = entry.Value;
        }

        return snapshot;
    }
}
