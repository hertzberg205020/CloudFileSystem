using CloudFileSystem.ConsoleApp.Visitors;
using FluentAssertions;
using Xunit;

namespace CloudFileSystem.Tests.Visitors;

public class SearchByExtensionVisitorTests
{
    [Fact]
    public void Visit_SearchDocx_FindsAllWordDocuments()
    {
        var console = new TestConsole();
        var visitor = new SearchByExtensionVisitor(".docx", console);
        var root = SampleStructureFactory.CreateSampleRoot();

        root.Accept(visitor);

        visitor.Results.Should().HaveCount(2);
        visitor.Results.Should().Contain(r => r.Contains("需求規格書.docx"));
        visitor.Results.Should().Contain(r => r.Contains("舊會議記錄.docx"));
    }

    [Fact]
    public void Visit_SearchTxt_FindsTextFiles()
    {
        var console = new TestConsole();
        var visitor = new SearchByExtensionVisitor(".txt", console);
        var root = SampleStructureFactory.CreateSampleRoot();

        root.Accept(visitor);

        visitor.Results.Should().HaveCount(2);
        visitor.Results.Should().Contain(r => r.Contains("待辦清單.txt"));
        visitor.Results.Should().Contain(r => r.Contains("README.txt"));
    }

    [Fact]
    public void Visit_SearchNonExistentExtension_ReturnsEmpty()
    {
        var console = new TestConsole();
        var visitor = new SearchByExtensionVisitor(".pdf", console);
        var root = SampleStructureFactory.CreateSampleRoot();

        root.Accept(visitor);

        visitor.Results.Should().BeEmpty();
    }

    [Fact]
    public void Visit_PrintsTraverseLog()
    {
        var console = new TestConsole();
        var visitor = new SearchByExtensionVisitor(".docx", console);
        var root = SampleStructureFactory.CreateSampleRoot();

        root.Accept(visitor);

        console.Output.Should().Contain("Visiting:");
    }

    [Fact]
    public void Constructor_WithoutDotPrefix_NormalizesExtension()
    {
        var console = new TestConsole();
        var visitor = new SearchByExtensionVisitor("png", console);
        var root = SampleStructureFactory.CreateSampleRoot();

        root.Accept(visitor);

        visitor.Results.Should().HaveCount(1);
        visitor.Results.Should().Contain(r => r.Contains("系統架構圖.png"));
    }

    [Fact]
    public void Visit_SearchUpperCaseExtension_FindsCaseInsensitively()
    {
        var console = new TestConsole();
        var visitor = new SearchByExtensionVisitor(".DOCX", console);
        var root = SampleStructureFactory.CreateSampleRoot();

        root.Accept(visitor);

        visitor.Results.Should().HaveCount(2);
        visitor.Results.Should().Contain(r => r.Contains("需求規格書.docx"));
        visitor.Results.Should().Contain(r => r.Contains("舊會議記錄.docx"));
    }
}
