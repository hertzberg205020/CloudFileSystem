using CloudFileSystem.ConsoleApp.Commands;
using CloudFileSystem.ConsoleApp.Models;
using FluentAssertions;
using Xunit;
using Directory = CloudFileSystem.ConsoleApp.Models.Directory;

namespace CloudFileSystem.Tests.Commands;

public class DeleteCommandTests
{
    private static readonly DateTime TestDate = new(2025, 1, 1);

    [Fact]
    public void Execute_RemovesComponentFromParent()
    {
        var dir = new Directory("Root");
        var file = new TextFile("test.txt", 100, TestDate, "UTF-8");
        dir.Add(file);
        var command = new DeleteCommand(dir, file);

        command.Execute();

        dir.Children.Should().BeEmpty();
    }

    [Fact]
    public void Undo_RestoresComponentAtOriginalIndex()
    {
        var dir = new Directory("Root");
        var first = new TextFile("first.txt", 100, TestDate, "UTF-8");
        var second = new TextFile("second.txt", 200, TestDate, "UTF-8");
        var third = new TextFile("third.txt", 300, TestDate, "UTF-8");
        dir.Add(first);
        dir.Add(second);
        dir.Add(third);
        var command = new DeleteCommand(dir, second);

        command.Execute();
        command.Undo();

        dir.Children[1].Should().BeSameAs(second);
        second.Parent.Should().BeSameAs(dir);
    }
}
