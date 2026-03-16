using CloudFileSystem.ConsoleApp.Visitors;
using FluentAssertions;
using Xunit;

namespace CloudFileSystem.Tests.Visitors;

public class XmlExportVisitorTests
{
    [Fact]
    public void Visit_SampleStructure_MatchesGoldenFile()
    {
        var visitor = new XmlExportVisitor();
        var root = SampleStructureFactory.CreateSampleRoot();

        root.Accept(visitor);

        var expected = System.IO.File.ReadAllText(
            Path.Combine(GetTestCasesPath(), "xml-format.out")
        );
        NormalizeWhitespace(visitor.GetXml()).Should().Be(NormalizeWhitespace(expected));
    }

    [Fact]
    public void Visit_SanitizesTagNames_CorrectlyFormatsXml()
    {
        var visitor = new XmlExportVisitor();
        var root = SampleStructureFactory.CreateSampleRoot();

        root.Accept(visitor);

        var xml = visitor.GetXml();
        xml.Should().Contain("<根目錄_Root>");
        xml.Should().Contain("</根目錄_Root>");
        xml.Should().Contain("<專案文件_Project_Docs>");
        xml.Should().Contain("<需求規格書_docx>");
        xml.Should().Contain("<Archive_2025>");
    }

    private static string NormalizeWhitespace(string text) =>
        string.Join("\n", text.TrimEnd().Split('\n').Select(l => l.TrimEnd()));

    private static string GetTestCasesPath()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null && !System.IO.File.Exists(Path.Combine(dir, "CloudFileSystem.sln")))
            dir = System.IO.Directory.GetParent(dir)?.FullName;
        return Path.Combine(dir!, "test-cases");
    }
}
