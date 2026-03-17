using CloudFileSystem.ConsoleApp.Models;
using FluentAssertions;
using Xunit;
using Directory = CloudFileSystem.ConsoleApp.Models.Directory;

namespace CloudFileSystem.Tests.Models;

public class DirectoryValidationTests
{
    private static readonly DateTime TestDate = new(2025, 1, 1);

    [Fact]
    public void Add_Null_ThrowsArgumentNullException()
    {
        var dir = new Directory("root");

        var act = () => dir.Add(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Add_DuplicateName_ThrowsInvalidOperationException()
    {
        var dir = new Directory("root");
        dir.Add(new TextFile("file.txt", 100, TestDate, "UTF-8"));

        var act = () => dir.Add(new TextFile("file.txt", 200, TestDate, "UTF-8"));

        act.Should().Throw<InvalidOperationException>().WithMessage("*file.txt*");
    }

    [Fact]
    public void Add_Self_ThrowsInvalidOperationException()
    {
        var dir = new Directory("root");

        var act = () => dir.Add(dir);

        act.Should().Throw<InvalidOperationException>().WithMessage("*自己*");
    }

    [Fact]
    public void Add_AncestorDirectory_ThrowsInvalidOperationException()
    {
        var root = new Directory("root");
        var child = new Directory("child");
        var grandchild = new Directory("grandchild");
        root.Add(child);
        child.Add(grandchild);

        var act = () => grandchild.Add(root);

        act.Should().Throw<InvalidOperationException>().WithMessage("*祖先*");
    }

    [Fact]
    public void Remove_Null_ThrowsArgumentNullException()
    {
        var dir = new Directory("root");

        var act = () => dir.Remove(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Remove_NonExistentComponent_ThrowsInvalidOperationException()
    {
        var dir = new Directory("root");
        var file = new TextFile("file.txt", 100, TestDate, "UTF-8");

        var act = () => dir.Remove(file);

        act.Should().Throw<InvalidOperationException>().WithMessage("*不在此目錄*");
    }

    [Fact]
    public void Remove_ExistingComponent_ClearsParentAfterRemoval()
    {
        var dir = new Directory("root");
        var file = new TextFile("file.txt", 100, TestDate, "UTF-8");
        dir.Add(file);

        dir.Remove(file);

        file.Parent.Should().BeNull();
        dir.Children.Should().BeEmpty();
    }

    [Fact]
    public void Insert_Null_ThrowsArgumentNullException()
    {
        var dir = new Directory("root");

        var act = () => dir.Insert(0, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Insert_NegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        var dir = new Directory("root");
        var file = new TextFile("file.txt", 100, TestDate, "UTF-8");

        var act = () => dir.Insert(-1, file);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Insert_IndexBeyondCount_ThrowsArgumentOutOfRangeException()
    {
        var dir = new Directory("root");
        var file = new TextFile("file.txt", 100, TestDate, "UTF-8");

        var act = () => dir.Insert(1, file);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetChildrenOrder_Null_ThrowsArgumentNullException()
    {
        var dir = new Directory("root");

        var act = () => dir.SetChildrenOrder(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SetChildrenOrder_MismatchedContent_ThrowsArgumentException()
    {
        var dir = new Directory("root");
        var file1 = new TextFile("a.txt", 100, TestDate, "UTF-8");
        var file2 = new TextFile("b.txt", 200, TestDate, "UTF-8");
        dir.Add(file1);
        dir.Add(file2);

        var stranger = new TextFile("c.txt", 300, TestDate, "UTF-8");
        var act = () => dir.SetChildrenOrder(new List<FileSystemComponent> { file1, stranger });

        act.Should().Throw<ArgumentException>().WithMessage("*完全相同*");
    }

    [Fact]
    public void SetChildrenOrder_DifferentCount_ThrowsArgumentException()
    {
        var dir = new Directory("root");
        var file1 = new TextFile("a.txt", 100, TestDate, "UTF-8");
        dir.Add(file1);

        var act = () => dir.SetChildrenOrder(new List<FileSystemComponent>());

        act.Should().Throw<ArgumentException>().WithMessage("*完全相同*");
    }
}
