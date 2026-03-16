using CloudFileSystem.ConsoleApp.Models;
using CloudFileSystem.ConsoleApp.Visitors;
using FluentAssertions;
using Xunit;
using Directory = CloudFileSystem.ConsoleApp.Models.Directory;

namespace CloudFileSystem.Tests.Visitors;

public class SizeCalculatorVisitorTests
{
    [Fact]
    public void Visit_SampleStructure_ReturnsCorrectTotalSize()
    {
        var console = new TestConsole();
        var visitor = new SizeCalculatorVisitor(console);
        var root = SampleStructureFactory.CreateSampleRoot();

        root.Accept(visitor);

        var expectedSize = 500L * 1024 + 2 * 1024 * 1024 + 1024 + 200 * 1024 + 500;
        visitor.TotalSize.Should().Be(expectedSize);
    }

    [Fact]
    public void Visit_EmptyDirectory_ReturnsZero()
    {
        var console = new TestConsole();
        var visitor = new SizeCalculatorVisitor(console);
        var root = new Directory("Empty");

        root.Accept(visitor);

        visitor.TotalSize.Should().Be(0);
    }

    [Fact]
    public void Visit_PrintsTraverseLog()
    {
        var console = new TestConsole();
        var visitor = new SizeCalculatorVisitor(console);
        var root = new Directory("Root");
        root.Add(new TextFile("a.txt", 100, DateTime.Now, "UTF-8"));

        root.Accept(visitor);

        console.Output.Should().Contain("Visiting: Root");
        console.Output.Should().Contain("Visiting: Root/a.txt");
    }
}
