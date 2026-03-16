using CloudFileSystem.ConsoleApp.Models;

namespace CloudFileSystem.ConsoleApp.Visitors;

/// <summary>
/// 以樹狀結構格式顯示檔案系統的 Visitor，對應功能一「目錄結構呈現」。
/// </summary>
/// <remarks>
/// 輸出格式使用 <c>├──</c>、<c>└──</c> 等 Box-drawing 字元繪製樹狀縮排。
/// </remarks>
public class DisplayVisitor : IFileSystemVisitor
{
    private readonly IConsole _console;
    private readonly List<bool> _isLast = [];

    /// <summary>
    /// 初始化 <see cref="DisplayVisitor"/> 的新執行個體。
    /// </summary>
    /// <param name="console">用於輸出樹狀結構的 Console 抽象。</param>
    public DisplayVisitor(IConsole console)
    {
        _console = console;
    }

    /// <inheritdoc/>
    public void Visit(Directory directory)
    {
        if (_isLast.Count == 0)
        {
            _console.WriteLine(directory.Name);
        }
        else
        {
            var tag = directory.Parent?.Parent == null ? "目錄" : "子目錄";
            PrintPrefix();
            _console.WriteLine($"{directory.Name} [{tag}]");
        }

        var children = directory.Children.ToList();
        for (var i = 0; i < children.Count; i++)
        {
            _isLast.Add(i == children.Count - 1);
            children[i].Accept(this);
            _isLast.RemoveAt(_isLast.Count - 1);
        }
    }

    /// <inheritdoc/>
    public void Visit(WordDocument file)
    {
        PrintPrefix();
        _console.WriteLine(
            $"{file.Name} [Word 檔案] (頁數: {file.PageCount}, 大小: {File.FormatSize(file.Size)})"
        );
    }

    /// <inheritdoc/>
    public void Visit(ImageFile file)
    {
        PrintPrefix();
        _console.WriteLine(
            $"{file.Name} [圖片] (解析度: {file.Width}x{file.Height}, 大小: {File.FormatSize(file.Size)})"
        );
    }

    /// <inheritdoc/>
    public void Visit(TextFile file)
    {
        PrintPrefix();
        _console.WriteLine(
            $"{file.Name} [純文字檔] (編碼: {file.Encoding}, 大小: {File.FormatSize(file.Size)})"
        );
    }

    private void PrintPrefix()
    {
        for (var i = 0; i < _isLast.Count - 1; i++)
        {
            _console.Write(_isLast[i] ? "    " : "│   ");
        }

        if (_isLast.Count > 0)
        {
            _console.Write(_isLast[^1] ? "└── " : "├── ");
        }
    }
}
