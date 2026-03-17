using CloudFileSystem.ConsoleApp.Models;

namespace CloudFileSystem.ConsoleApp.Commands;

/// <summary>
/// 將 clipboard 內容深拷貝後貼入目標目錄的 Command。同名時自動改名。
/// </summary>
public class PasteCommand : ICommand
{
    private readonly Directory _target;
    private readonly FileSystemComponent _clipboard;
    private FileSystemComponent? _cloned;

    /// <summary>
    /// 初始化 <see cref="PasteCommand"/> 的新執行個體。
    /// </summary>
    /// <param name="target">要貼入的目標目錄。</param>
    /// <param name="clipboard">clipboard 中的元件參考。</param>
    public PasteCommand(Directory target, FileSystemComponent clipboard)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(clipboard);
        _target = target;
        _clipboard = clipboard;
    }

    /// <inheritdoc/>
    public string Description => $"Paste {_clipboard.Name}";

    /// <inheritdoc/>
    public void Execute()
    {
        if (_cloned == null)
        {
            _cloned = _clipboard.DeepCopy();
            _cloned.Name = GenerateUniqueName(_cloned.Name);
        }

        _target.Add(_cloned);
    }

    /// <inheritdoc/>
    public void Undo()
    {
        if (_cloned != null)
            _target.Remove(_cloned);
    }

    private string GenerateUniqueName(string name)
    {
        var existingNames = _target.Children.Select(c => c.Name).ToHashSet();
        if (!existingNames.Contains(name))
            return name;

        var extension = Path.GetExtension(name);
        var baseName = Path.GetFileNameWithoutExtension(name);

        const int maxRetries = 1000;
        for (var i = 1; i <= maxRetries; i++)
        {
            var candidate = $"{baseName} ({i}){extension}";
            if (!existingNames.Contains(candidate))
                return candidate;
        }

        throw new InvalidOperationException("無法產生唯一名稱");
    }
}
