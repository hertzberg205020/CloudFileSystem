using CloudFileSystem.ConsoleApp.Commands;
using CloudFileSystem.ConsoleApp.Models;
using FluentAssertions;
using Xunit;
using Directory = CloudFileSystem.ConsoleApp.Models.Directory;

namespace CloudFileSystem.Tests.Commands;

public class SortCommandTests
{
    private static readonly DateTime TestDate = new(2025, 1, 1);

    [Fact]
    public void Execute_SortsByNameAsc()
    {
        var dir = new Directory("Root");
        dir.Add(new TextFile("c.txt", 100, TestDate, "UTF-8"));
        dir.Add(new TextFile("a.txt", 200, TestDate, "UTF-8"));
        dir.Add(new TextFile("b.txt", 300, TestDate, "UTF-8"));
        var command = new SortCommand(dir, SortBy.Name, SortOrder.Asc);

        command.Execute();

        dir.Children.Select(c => c.Name)
            .Should()
            .ContainInOrder("a.txt", "b.txt", "c.txt");
    }

    [Fact]
    public void Undo_RestoresOriginalOrder()
    {
        var dir = new Directory("Root");
        dir.Add(new TextFile("c.txt", 100, TestDate, "UTF-8"));
        dir.Add(new TextFile("a.txt", 200, TestDate, "UTF-8"));
        dir.Add(new TextFile("b.txt", 300, TestDate, "UTF-8"));
        var command = new SortCommand(dir, SortBy.Name, SortOrder.Asc);

        command.Execute();
        command.Undo();

        dir.Children.Select(c => c.Name)
            .Should()
            .ContainInOrder("c.txt", "a.txt", "b.txt");
    }

    [Fact]
    public void Undo_RecursiveSort_RestoresSubDirectoryOrder()
    {
        var root = new Directory("Root");
        var sub = new Directory("Sub");
        sub.Add(new TextFile("z.txt", 100, TestDate, "UTF-8"));
        sub.Add(new TextFile("a.txt", 200, TestDate, "UTF-8"));
        root.Add(sub);
        var command = new SortCommand(root, SortBy.Name, SortOrder.Asc);

        command.Execute();
        sub.Children.Select(c => c.Name).Should().ContainInOrder("a.txt", "z.txt");

        command.Undo();
        sub.Children.Select(c => c.Name).Should().ContainInOrder("z.txt", "a.txt");
    }
}
