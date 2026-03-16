using System.Text;
using CloudFileSystem.ConsoleApp.Models;

namespace CloudFileSystem.ConsoleApp.Visitors;

/// <summary>
/// 以樹狀結構格式顯示檔案系統的 Visitor，對應功能一「目錄結構呈現」。
/// </summary>
/// <remarks>
/// <para>輸出格式使用 <c>├──</c>、<c>└──</c> 等 Box-drawing 字元繪製樹狀縮排。</para>
/// <para>走訪完成後透過 <see cref="GetOutput"/> 取得產生的樹狀結構字串。</para>
/// </remarks>
public class DisplayVisitor : IFileSystemVisitor
{
    private readonly StringBuilder _outputBuilder = new();
    private readonly List<bool> _isLast = [];

    /// <summary>
    /// 取得走訪後產生的樹狀結構字串。
    /// </summary>
    /// <returns>完整的樹狀結構文字。</returns>
    public string GetOutput() => _outputBuilder.ToString();

    /// <inheritdoc/>
    public void Visit(Directory directory)
    {
        if (_isLast.Count == 0)
        {
            _outputBuilder.Append(directory.Name);
            AppendTags(directory);
            _outputBuilder.AppendLine();
        }
        else
        {
            var tag = directory.Parent?.Parent == null ? "目錄" : "子目錄";
            AppendPrefix();
            _outputBuilder.Append($"{directory.Name} [{tag}]");
            AppendTags(directory);
            _outputBuilder.AppendLine();
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
        AppendPrefix();
        _outputBuilder.Append(
            $"{file.Name} [Word 檔案] (頁數: {file.PageCount}, 大小: {File.FormatSize(file.Size)})"
        );
        AppendTags(file);
        _outputBuilder.AppendLine();
    }

    /// <inheritdoc/>
    public void Visit(ImageFile file)
    {
        AppendPrefix();
        _outputBuilder.Append(
            $"{file.Name} [圖片] (解析度: {file.Width}x{file.Height}, 大小: {File.FormatSize(file.Size)})"
        );
        AppendTags(file);
        _outputBuilder.AppendLine();
    }

    /// <inheritdoc/>
    public void Visit(TextFile file)
    {
        AppendPrefix();
        _outputBuilder.Append(
            $"{file.Name} [純文字檔] (編碼: {file.Encoding}, 大小: {File.FormatSize(file.Size)})"
        );
        AppendTags(file);
        _outputBuilder.AppendLine();
    }

    private void AppendTags(FileSystemComponent component)
    {
        if (component.Tags.Count > 0)
        {
            var sorted = component.Tags.OrderBy(t => t).Select(t => t.ToString());
            _outputBuilder.Append($" {{{string.Join(", ", sorted)}}}");
        }
    }

    private void AppendPrefix()
    {
        for (var i = 0; i < _isLast.Count - 1; i++)
        {
            _outputBuilder.Append(_isLast[i] ? "    " : "│   ");
        }

        if (_isLast.Count > 0)
        {
            _outputBuilder.Append(_isLast[^1] ? "└── " : "├── ");
        }
    }
}
