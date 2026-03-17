using CloudFileSystem.ConsoleApp.Commands;
using CloudFileSystem.ConsoleApp.Models;
using FluentAssertions;
using Xunit;
using Directory = CloudFileSystem.ConsoleApp.Models.Directory;

namespace CloudFileSystem.Tests.Commands;

public class CommandValidationTests
{
    private static readonly DateTime TestDate = new(2025, 1, 1);

    // === CommandManager ===

    [Fact]
    public void CommandManager_ExecuteNull_ThrowsArgumentNullException()
    {
        var manager = new CommandManager();

        var act = () => manager.Execute(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // === DeleteCommand ===

    [Fact]
    public void DeleteCommand_NullParent_ThrowsArgumentNullException()
    {
        var file = new TextFile("test.txt", 100, TestDate, "UTF-8");

        var act = () => new DeleteCommand(null!, file);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DeleteCommand_NullComponent_ThrowsArgumentNullException()
    {
        var dir = new Directory("root");

        var act = () => new DeleteCommand(dir, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DeleteCommand_ComponentNotInParent_ThrowsInvalidOperationException()
    {
        var dir = new Directory("root");
        var file = new TextFile("test.txt", 100, TestDate, "UTF-8");

        var act = () => new DeleteCommand(dir, file);

        act.Should().Throw<InvalidOperationException>().WithMessage("*不在*");
    }

    // === PasteCommand ===

    [Fact]
    public void PasteCommand_NullTarget_ThrowsArgumentNullException()
    {
        var file = new TextFile("test.txt", 100, TestDate, "UTF-8");

        var act = () => new PasteCommand(null!, file);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PasteCommand_NullClipboard_ThrowsArgumentNullException()
    {
        var dir = new Directory("root");

        var act = () => new PasteCommand(dir, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // === SortCommand ===

    [Fact]
    public void SortCommand_NullDirectory_ThrowsArgumentNullException()
    {
        var act = () => new SortCommand(null!, SortBy.Name, SortOrder.Asc);

        act.Should().Throw<ArgumentNullException>();
    }

    // === TagCommand ===

    [Fact]
    public void TagCommand_NullComponent_ThrowsArgumentNullException()
    {
        var act = () => new TagCommand(null!, Tag.Urgent);

        act.Should().Throw<ArgumentNullException>();
    }

    // === UntagCommand ===

    [Fact]
    public void UntagCommand_NullComponent_ThrowsArgumentNullException()
    {
        var act = () => new UntagCommand(null!, Tag.Urgent);

        act.Should().Throw<ArgumentNullException>();
    }
}
