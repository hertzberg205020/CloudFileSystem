using CloudFileSystem.ConsoleApp.Models;

namespace CloudFileSystem.ConsoleApp.Visitors;

public class DisplayVisitor : IFileSystemVisitor
{
    private readonly IConsole _console;
    private readonly List<bool> _isLast = [];

    public DisplayVisitor(IConsole console)
    {
        _console = console;
    }

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

    public void Visit(WordDocument file)
    {
        PrintPrefix();
        _console.WriteLine(
            $"{file.Name} [Word 檔案] (頁數: {file.PageCount}, 大小: {File.FormatSize(file.Size)})"
        );
    }

    public void Visit(ImageFile file)
    {
        PrintPrefix();
        _console.WriteLine(
            $"{file.Name} [圖片] (解析度: {file.Width}x{file.Height}, 大小: {File.FormatSize(file.Size)})"
        );
    }

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
