using CloudFileSystem.ConsoleApp.Commands;
using CloudFileSystem.ConsoleApp.Models;
using CloudFileSystem.ConsoleApp.Visitors;

namespace CloudFileSystem.ConsoleApp;

/// <summary>
/// 雲端檔案系統的互動式 CLI controller。
/// </summary>
/// <remarks>
/// <para>透過建構子注入 <see cref="IConsole"/> 與根目錄 <see cref="Directory"/>，
/// 持有 <see cref="CommandManager"/> 管理 undo/redo 歷史與 <c>_clipboard</c> 暫存複製內容。</para>
/// <para>唯讀指令直接執行，突變指令透過 <see cref="CommandManager"/> 執行。</para>
/// </remarks>
public class CloudFileSystemCli
{
    private readonly IConsole _console;
    private readonly Directory _root;
    private readonly CommandManager _commandManager = new();
    private FileSystemComponent? _clipboard;

    /// <summary>
    /// 初始化 <see cref="CloudFileSystemCli"/> 的新執行個體。
    /// </summary>
    /// <param name="root">檔案系統的根目錄。</param>
    /// <param name="console">用於 I/O 操作的 Console 抽象。</param>
    public CloudFileSystemCli(Directory root, IConsole console)
    {
        _root = root;
        _console = console;
    }

    /// <summary>
    /// 啟動互動式指令迴圈，持續接收並執行使用者指令，直到輸入 <c>exit</c> 或輸入結束。
    /// </summary>
    public void Start()
    {
        while (true)
        {
            _console.Write($"{_root.Name}> ");
            var input = _console.ReadLine();
            if (input == null)
                break;

            var command = input.Trim();
            if (string.IsNullOrEmpty(command))
                continue;

            if (command.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            ExecuteCommand(command);
        }
    }

    private void ExecuteCommand(string command)
    {
        var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var action = parts[0].ToLowerInvariant();

        switch (action)
        {
            // === 唯讀指令 ===
            case "display":
                HandleDisplay();
                break;

            case "size":
                HandleSize();
                break;

            case "search":
                HandleSearch(parts);
                break;

            case "xml":
                HandleXml();
                break;

            case "copy":
                HandleCopy(parts);
                break;

            // === 突變指令（透過 CommandManager）===
            case "delete":
                HandleDelete(parts);
                break;

            case "paste":
                HandlePaste();
                break;

            case "sort":
                HandleSort(parts);
                break;

            case "tag":
                HandleTag(parts);
                break;

            case "untag":
                HandleUntag(parts);
                break;

            // === 管理指令 ===
            case "undo":
                HandleUndo();
                break;

            case "redo":
                HandleRedo();
                break;

            default:
                _console.WriteError($"Unknown command: {action}");
                break;
        }
    }

    private void HandleDisplay()
    {
        var visitor = new DisplayVisitor();
        _root.Accept(visitor);
        _console.Write(visitor.GetOutput());
    }

    private void HandleSize()
    {
        var visitor = new SizeCalculatorVisitor(_console);
        _root.Accept(visitor);
        _console.WriteLine($"Total Size: {File.FormatSize(visitor.TotalSize)}");
    }

    private void HandleSearch(string[] parts)
    {
        if (parts.Length < 2)
        {
            _console.WriteError("Usage: search <extension>");
            return;
        }

        var visitor = new SearchByExtensionVisitor(parts[1], _console);
        _root.Accept(visitor);
        _console.WriteLine($"Found {visitor.Results.Count} file(s):");
        foreach (var result in visitor.Results)
            _console.WriteLine($"  {result}");
    }

    private void HandleXml()
    {
        var visitor = new XmlExportVisitor();
        _root.Accept(visitor);
        _console.WriteLine(visitor.GetXml().TrimEnd());
    }

    private void HandleCopy(string[] parts)
    {
        if (parts.Length < 2)
        {
            _console.WriteError("Usage: copy <name>");
            return;
        }

        var name = string.Join(' ', parts[1..]);
        var component = FindChild(name);
        if (component == null)
            return;

        _clipboard = component;
        _console.WriteLine($"Copied: {component.Name}");
    }

    private void HandleDelete(string[] parts)
    {
        if (parts.Length < 2)
        {
            _console.WriteError("Usage: delete <name>");
            return;
        }

        var name = string.Join(' ', parts[1..]);
        var component = FindChild(name);
        if (component == null)
            return;

        var parent = component.Parent ?? _root;
        _commandManager.Execute(new DeleteCommand(parent, component));
        _console.WriteLine($"Deleted: {component.Name}");
    }

    private void HandlePaste()
    {
        if (_clipboard == null)
        {
            _console.WriteError("Clipboard is empty.");
            return;
        }

        var command = new PasteCommand(_root, _clipboard);
        _commandManager.Execute(command);
        _console.WriteLine($"Pasted: {_clipboard.Name}");
    }

    private void HandleSort(string[] parts)
    {
        if (parts.Length < 3)
        {
            _console.WriteError("Usage: sort <name|size|ext> <asc|desc>");
            return;
        }

        if (!TryParseSortBy(parts[1], out var sortBy))
        {
            _console.WriteError($"Invalid sort field: {parts[1]}. Use: name, size, ext");
            return;
        }

        if (!TryParseSortOrder(parts[2], out var sortOrder))
        {
            _console.WriteError($"Invalid sort order: {parts[2]}. Use: asc, desc");
            return;
        }

        _commandManager.Execute(new SortCommand(_root, sortBy, sortOrder));
        _console.WriteLine($"Sorted by {sortBy} {sortOrder}");
    }

    private void HandleTag(string[] parts)
    {
        if (parts.Length < 3)
        {
            _console.WriteError("Usage: tag <name> <Urgent|Work|Personal>");
            return;
        }

        if (!TryParseTag(parts[^1], out var tag))
        {
            _console.WriteError($"Invalid tag: {parts[^1]}. Use: Urgent, Work, Personal");
            return;
        }

        var name = string.Join(' ', parts[1..^1]);
        var component = FindChild(name);
        if (component == null)
            return;

        _commandManager.Execute(new TagCommand(component, tag));
        _console.WriteLine($"Tagged {component.Name} as {tag}");
    }

    private void HandleUntag(string[] parts)
    {
        if (parts.Length < 3)
        {
            _console.WriteError("Usage: untag <name> <Urgent|Work|Personal>");
            return;
        }

        if (!TryParseTag(parts[^1], out var tag))
        {
            _console.WriteError($"Invalid tag: {parts[^1]}. Use: Urgent, Work, Personal");
            return;
        }

        var name = string.Join(' ', parts[1..^1]);
        var component = FindChild(name);
        if (component == null)
            return;

        _commandManager.Execute(new UntagCommand(component, tag));
        _console.WriteLine($"Removed {tag} from {component.Name}");
    }

    private void HandleUndo()
    {
        var command = _commandManager.Undo();
        if (command == null)
            _console.WriteError("Nothing to undo.");
        else
            _console.WriteLine($"Undone: {command.Description}");
    }

    private void HandleRedo()
    {
        var command = _commandManager.Redo();
        if (command == null)
            _console.WriteError("Nothing to redo.");
        else
            _console.WriteLine($"Redone: {command.Description}");
    }

    /// <summary>
    /// 依名稱或路徑查找元件。支援 <c>/</c> 分隔的多層路徑（如 <c>"目錄A/目錄B/檔案.txt"</c>），
    /// 不含 <c>/</c> 時僅搜尋根目錄的直接子元件。
    /// </summary>
    private FileSystemComponent? FindChild(string nameOrPath)
    {
        var segments = nameOrPath.Split('/');
        FileSystemComponent? current = null;
        var searchIn = _root.Children;

        foreach (var segment in segments)
        {
            current = searchIn.FirstOrDefault(c =>
                c.Name.Equals(segment, StringComparison.Ordinal));
            if (current == null)
            {
                _console.WriteError($"Not found: {nameOrPath}");
                return null;
            }

            if (current is Directory dir)
                searchIn = dir.Children;
        }

        return current;
    }

    private static bool TryParseSortBy(string value, out SortBy sortBy)
    {
        sortBy = value.ToLowerInvariant() switch
        {
            "name" => SortBy.Name,
            "size" => SortBy.Size,
            "ext" or "extension" => SortBy.Extension,
            _ => (SortBy)(-1),
        };
        return (int)sortBy >= 0;
    }

    private static bool TryParseSortOrder(string value, out SortOrder sortOrder)
    {
        sortOrder = value.ToLowerInvariant() switch
        {
            "asc" => SortOrder.Asc,
            "desc" => SortOrder.Desc,
            _ => (SortOrder)(-1),
        };
        return (int)sortOrder >= 0;
    }

    private static bool TryParseTag(string value, out Tag tag)
    {
        return Enum.TryParse(value, ignoreCase: true, out tag)
               && Enum.IsDefined(tag);
    }
}
