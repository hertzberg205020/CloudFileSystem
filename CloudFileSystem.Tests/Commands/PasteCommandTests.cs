using CloudFileSystem.ConsoleApp.Commands;
using CloudFileSystem.ConsoleApp.Models;
using FluentAssertions;
using Xunit;
using Directory = CloudFileSystem.ConsoleApp.Models.Directory;

namespace CloudFileSystem.Tests.Commands;

public class PasteCommandTests
{
    private static readonly DateTime TestDate = new(2025, 1, 1);

    [Fact]
    public void Execute_PastesDeepCopyIntoTarget()
    {
        var target = new Directory("Root");
        var clipboard = new TextFile("test.txt", 100, TestDate, "UTF-8");
        var command = new PasteCommand(target, clipboard);

        command.Execute();

        target.Children.Should().HaveCount(1);
        target.Children[0].Should().NotBeSameAs(clipboard);
        target.Children[0].Name.Should().Be("test.txt");
    }

    [Fact]
    public void Execute_SameNameExists_AutoRenames()
    {
        var target = new Directory("Root");
        target.Add(new TextFile("test.txt", 50, TestDate, "UTF-8"));
        var clipboard = new TextFile("test.txt", 100, TestDate, "UTF-8");
        var command = new PasteCommand(target, clipboard);

        command.Execute();

        target.Children.Should().HaveCount(2);
        target.Children[1].Name.Should().Be("test (1).txt");
    }

    [Fact]
    public void Execute_MultipleSameNames_IncrementsCounter()
    {
        var target = new Directory("Root");
        target.Add(new TextFile("test.txt", 50, TestDate, "UTF-8"));
        target.Add(new TextFile("test (1).txt", 50, TestDate, "UTF-8"));
        var clipboard = new TextFile("test.txt", 100, TestDate, "UTF-8");
        var command = new PasteCommand(target, clipboard);

        command.Execute();

        target.Children[2].Name.Should().Be("test (2).txt");
    }

    [Fact]
    public void Execute_PastesDirectory_DeepCopiesSubtree()
    {
        var target = new Directory("Root");
        var clipDir = new Directory("Sub");
        clipDir.Add(new TextFile("a.txt", 100, TestDate, "UTF-8"));
        var command = new PasteCommand(target, clipDir);

        command.Execute();

        var pasted = (Directory)target.Children[0];
        pasted.Children.Should().HaveCount(1);
        pasted.Children[0].Should().NotBeSameAs(clipDir.Children[0]);
    }

    [Fact]
    public void Undo_RemovesPastedComponent()
    {
        var target = new Directory("Root");
        var clipboard = new TextFile("test.txt", 100, TestDate, "UTF-8");
        var command = new PasteCommand(target, clipboard);
        command.Execute();

        command.Undo();

        target.Children.Should().BeEmpty();
    }

    [Fact]
    public void Redo_ReusesClonedObject_DoesNotDeepCopyAgain()
    {
        // Arrange
        var target = new Directory("Root");
        var clipboard = new TextFile("test.txt", 100, TestDate, "UTF-8");
        var command = new PasteCommand(target, clipboard);
        command.Execute();
        var firstCloned = target.Children[0];
        command.Undo();

        // Act — redo
        command.Execute();

        // Assert — same object reference, no new deep copy
        target.Children.Should().HaveCount(1);
        target.Children[0].Should().BeSameAs(firstCloned);
    }

    [Fact]
    public void Redo_SameDirectoryCopy_PreservesOriginalName()
    {
        // Arrange — paste into directory that already has same-named file
        var target = new Directory("Root");
        var original = new TextFile("test.txt", 50, TestDate, "UTF-8");
        target.Add(original);
        var command = new PasteCommand(target, original);
        command.Execute(); // produces "test (1).txt"
        var pastedName = target.Children[1].Name;
        pastedName.Should().Be("test (1).txt");
        command.Undo();

        // Act — redo should produce the same name
        command.Execute();

        // Assert
        target.Children[1].Name.Should().Be(pastedName);
    }

    [Fact]
    public void Redo_MultipleUndoRedoCycles_AlwaysReuseSameObject()
    {
        // Arrange
        var target = new Directory("Root");
        var clipboard = new TextFile("test.txt", 100, TestDate, "UTF-8");
        var command = new PasteCommand(target, clipboard);
        command.Execute();
        var firstCloned = target.Children[0];

        // Act — multiple undo/redo cycles
        command.Undo();
        command.Execute(); // redo 1
        command.Undo();
        command.Execute(); // redo 2

        // Assert — still the same object
        target.Children.Should().HaveCount(1);
        target.Children[0].Should().BeSameAs(firstCloned);
    }
}
