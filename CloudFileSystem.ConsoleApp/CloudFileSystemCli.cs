using CloudFileSystem.ConsoleApp.Models;
using CloudFileSystem.ConsoleApp.Visitors;

namespace CloudFileSystem.ConsoleApp;

/// <summary>
/// 雲端檔案系統的互動式 CLI controller。
/// </summary>
/// <remarks>
/// <para>透過建構子注入 <see cref="IConsole"/> 與根目錄 <see cref="Directory"/>，
/// 採用指令迴圈模式接收使用者輸入並分派至對應的 <see cref="IFileSystemVisitor"/>。</para>
/// <para>支援的指令：<c>display</c>、<c>size</c>、<c>search</c>、<c>xml</c>、<c>exit</c>。</para>
/// </remarks>
public class CloudFileSystemCli
{
    private readonly IConsole _console;
    private readonly Directory _root;

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

            ExecuteCommand(command);
        }
    }

    private void ExecuteCommand(string command)
    {
        var parts = command.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var action = parts[0].ToLowerInvariant();

        switch (action)
        {
            case "display":
                var displayVisitor = new DisplayVisitor();
                _root.Accept(displayVisitor);
                _console.Write(displayVisitor.GetOutput());
                break;

            case "size":
                var sizeVisitor = new SizeCalculatorVisitor(_console);
                _root.Accept(sizeVisitor);
                _console.WriteLine($"Total Size: {File.FormatSize(sizeVisitor.TotalSize)}");
                break;

            case "search":
                if (parts.Length < 2)
                {
                    _console.WriteError("Usage: search <extension>");
                    break;
                }

                var searchVisitor = new SearchByExtensionVisitor(parts[1], _console);
                _root.Accept(searchVisitor);
                _console.WriteLine($"Found {searchVisitor.Results.Count} file(s):");
                foreach (var result in searchVisitor.Results)
                    _console.WriteLine($"  {result}");
                break;

            case "xml":
                var xmlVisitor = new XmlExportVisitor();
                _root.Accept(xmlVisitor);
                _console.WriteLine(xmlVisitor.GetXml().TrimEnd());
                break;

            case "exit":
                return;

            default:
                _console.WriteError($"Unknown command: {action}");
                break;
        }
    }
}
