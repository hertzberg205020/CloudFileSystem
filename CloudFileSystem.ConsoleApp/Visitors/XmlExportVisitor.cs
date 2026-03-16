using System.Text;
using CloudFileSystem.ConsoleApp.Models;

namespace CloudFileSystem.ConsoleApp.Visitors;

public class XmlExportVisitor : IFileSystemVisitor
{
    private readonly StringBuilder _xmlBuilder = new();
    private int _indentLevel;

    public string GetXml() => _xmlBuilder.ToString();

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

    public void Visit(WordDocument file)
    {
        var tagName = SanitizeTagName(file.Name);
        AppendLine(
            $"<{tagName}>頁數: {file.PageCount}, 大小: {File.FormatSize(file.Size)}</{tagName}>"
        );
    }

    public void Visit(ImageFile file)
    {
        var tagName = SanitizeTagName(file.Name);
        AppendLine(
            $"<{tagName}>解析度: {file.Width}x{file.Height}, 大小: {File.FormatSize(file.Size)}</{tagName}>"
        );
    }

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
