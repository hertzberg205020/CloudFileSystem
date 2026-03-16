using CloudFileSystem.ConsoleApp.Models;

namespace CloudFileSystem.ConsoleApp.Commands;

/// <summary>
/// 移除元件標籤的 Command，Undo 時加回該標籤。
/// </summary>
public class UntagCommand : ICommand
{
    private readonly FileSystemComponent _component;
    private readonly Tag _tag;

    /// <summary>
    /// 初始化 <see cref="UntagCommand"/> 的新執行個體。
    /// </summary>
    /// <param name="component">要移除標籤的元件。</param>
    /// <param name="tag">要移除的標籤。</param>
    public UntagCommand(FileSystemComponent component, Tag tag)
    {
        _component = component;
        _tag = tag;
    }

    /// <inheritdoc/>
    public string Description => $"Untag {_tag} from {_component.Name}";

    /// <inheritdoc/>
    public void Execute() => _component.RemoveTag(_tag);

    /// <inheritdoc/>
    public void Undo() => _component.AddTag(_tag);
}
