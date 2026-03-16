using CloudFileSystem.ConsoleApp.Models;
using CloudFileSystem.ConsoleApp.Visitors;
using FluentAssertions;
using Xunit;
using Directory = CloudFileSystem.ConsoleApp.Models.Directory;

namespace CloudFileSystem.Tests.Visitors;

public class DisplayVisitorTests
{
    [Fact]
    public void Visit_SampleStructure_MatchesGoldenFile()
    {
        var console = new TestConsole();
        var visitor = new DisplayVisitor(console);
        var root = SampleStructureFactory.CreateSampleRoot();

        root.Accept(visitor);

        var expected = System.IO.File.ReadAllText(
            Path.Combine(GetTestCasesPath(), "plain-text.out")
        );
        console.Output.TrimEnd().Should().Be(expected.TrimEnd());
    }

    [Fact]
    public void Visit_EmptyDirectory_PrintsOnlyDirectoryName()
    {
        var console = new TestConsole();
        var visitor = new DisplayVisitor(console);
        var root = new Directory("TestRoot");

        root.Accept(visitor);

        console.Output.TrimEnd().Should().Be("TestRoot");
    }

    [Fact]
    public void Visit_DirectoryWithSingleFile_PrintsCorrectTree()
    {
        var console = new TestConsole();
        var visitor = new DisplayVisitor(console);
        var root = new Directory("Root");
        root.Add(new TextFile("test.txt", 1024, DateTime.Now, "UTF-8"));

        root.Accept(visitor);

        var lines = console.Output.TrimEnd().Split(Environment.NewLine);
        lines[0].Should().Be("Root");
        lines[1].Should().Be("└── test.txt [純文字檔] (編碼: UTF-8, 大小: 1KB)");
    }

    private static string GetTestCasesPath()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null && !System.IO.File.Exists(Path.Combine(dir, "CloudFileSystem.sln")))
            dir = System.IO.Directory.GetParent(dir)?.FullName;
        return Path.Combine(dir!, "test-cases");
    }
}
