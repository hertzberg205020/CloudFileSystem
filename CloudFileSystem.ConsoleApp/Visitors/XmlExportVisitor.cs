using System.Text;
using CloudFileSystem.ConsoleApp.Models;

namespace CloudFileSystem.ConsoleApp.Visitors;

/// <summary>
/// 將檔案系統結構匯出為 XML 格式的 Visitor，對應功能二-3。
/// </summary>
/// <remarks>
/// 走訪完成後透過 <see cref="GetXml"/> 取得產生的 XML 字串。
/// 元件名稱中的特殊字元會經過清理以產生合法的 XML 標籤名稱。
/// </remarks>
public class XmlExportVisitor : IFileSystemVisitor
{
    private readonly StringBuilder _xmlBuilder = new();
    private int _indentLevel;

    /// <summary>
    /// 取得走訪後產生的 XML 字串。
    /// </summary>
    /// <returns>完整的 XML 結構字串。</returns>
    public string GetXml() => _xmlBuilder.ToString();

    /// <inheritdoc/>
    public void Visit(Directory directory)
    {
        var tagName = SanitizeTagName(directory.Name);
        AppendLine($"<{tagName}>");
        _indentLevel++;
        foreach (var child in directory.Children)
            child.Accept(this);
        _indentLevel--;
        AppendLine($"</{tagName}>");
    }

    /// <inheritdoc/>
    public void Visit(WordDocument file)
    {
        var tagName = SanitizeTagName(file.Name);
        AppendLine(
            $"<{tagName}>頁數: {file.PageCount}, 大小: {File.FormatSize(file.Size)}</{tagName}>"
        );
    }

    /// <inheritdoc/>
    public void Visit(ImageFile file)
    {
        var tagName = SanitizeTagName(file.Name);
        AppendLine(
            $"<{tagName}>解析度: {file.Width}x{file.Height}, 大小: {File.FormatSize(file.Size)}</{tagName}>"
        );
    }

    /// <inheritdoc/>
    public void Visit(TextFile file)
    {
        var tagName = SanitizeTagName(file.Name);
        AppendLine(
            $"<{tagName}>編碼: {file.Encoding}, 大小: {File.FormatSize(file.Size)}</{tagName}>"
        );
    }

    private void AppendLine(string line)
    {
        _xmlBuilder.Append(new string(' ', _indentLevel * 4));
        _xmlBuilder.AppendLine(line);
    }

    private static string SanitizeTagName(string name)
    {
        // Replace dots with underscores, remove spaces and parentheses content
        // "需求規格書.docx" → "需求規格書_docx"
        // "根目錄 (Root)" → "根目錄_Root"
        // "專案文件 (Project_Docs)" → "專案文件_Project_Docs"
        // "2025備份 (Archive_2025)" → "Archive_2025"

        // Check if name contains parentheses with English identifier
        var parenStart = name.IndexOf('(');
        var parenEnd = name.IndexOf(')');
        if (parenStart >= 0 && parenEnd > parenStart)
        {
            var englishPart = name[(parenStart + 1)..parenEnd].Trim();
            var chinesePart = name[..parenStart].Trim();

            // If Chinese part is purely numeric prefix like "2025備份", use English part only
            if (chinesePart.Any(char.IsDigit) && !chinesePart.Contains('_'))
                return englishPart;

            return $"{chinesePart}_{englishPart}";
        }

        // Replace dots with underscores for file names
        return name.Replace('.', '_');
    }
}
