using CloudFileSystem.ConsoleApp.Models;

namespace CloudFileSystem.ConsoleApp.Commands;

/// <summary>
/// 從目錄中刪除指定元件的 Command，Undo 時插回原位置。
/// </summary>
public class DeleteCommand : ICommand
{
    private readonly Directory _parent;
    private readonly FileSystemComponent _component;
    private readonly int _originalIndex;

    /// <summary>
    /// 初始化 <see cref="DeleteCommand"/> 的新執行個體。
    /// </summary>
    /// <param name="parent">元件所在的父目錄。</param>
    /// <param name="component">要刪除的元件。</param>
    public DeleteCommand(Directory parent, FileSystemComponent component)
    {
        _parent = parent;
        _component = component;
        _originalIndex = parent.IndexOf(component);
    }

    /// <inheritdoc/>
    public string Description => $"Delete {_component.Name}";

    /// <inheritdoc/>
    public void Execute() => _parent.Remove(_component);

    /// <inheritdoc/>
    public void Undo() => _parent.Insert(_originalIndex, _component);
}
