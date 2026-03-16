using CloudFileSystem.ConsoleApp.Models;

namespace CloudFileSystem.ConsoleApp.Commands;

/// <summary>
/// 為元件加上標籤的 Command，Undo 時移除該標籤。
/// </summary>
public class TagCommand : ICommand
{
    private readonly FileSystemComponent _component;
    private readonly Tag _tag;

    /// <summary>
    /// 初始化 <see cref="TagCommand"/> 的新執行個體。
    /// </summary>
    /// <param name="component">要加標籤的元件。</param>
    /// <param name="tag">要加入的標籤。</param>
    public TagCommand(FileSystemComponent component, Tag tag)
    {
        _component = component;
        _tag = tag;
    }

    /// <inheritdoc/>
    public string Description => $"Tag {_component.Name} as {_tag}";

    /// <inheritdoc/>
    public void Execute() => _component.AddTag(_tag);

    /// <inheritdoc/>
    public void Undo() => _component.RemoveTag(_tag);
}
