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

    [Fact]
    public void Execute_OriginalTaggedAfterSnapshot_PasteReflectsSnapshotState()
    {
        // Arrange
        var target = new Directory("Root");
        var file = new TextFile("test.txt", 100, TestDate, "UTF-8");
        file.AddTag(Tag.Urgent);
        var snapshot = file.DeepCopy(); // 快照時有 Urgent
        file.RemoveTag(Tag.Urgent); // 原件移除 Urgent
        var command = new PasteCommand(target, snapshot);

        // Act
        command.Execute();

        // Assert — 貼上的副本反映快照時的狀態（有 Urgent）
        target.Children[0].Tags.Should().Contain(Tag.Urgent);
    }

    [Fact]
    public void Execute_DirectoryChildAddedAfterSnapshot_PasteReflectsSnapshotChildren()
    {
        // Arrange
        var target = new Directory("Root");
        var dir = new Directory("Sub");
        dir.Add(new TextFile("a.txt", 100, TestDate, "UTF-8"));
        dir.Add(new TextFile("b.txt", 200, TestDate, "UTF-8"));
        var snapshot = dir.DeepCopy(); // 快照時有 2 children
        dir.Add(new TextFile("c.txt", 300, TestDate, "UTF-8")); // 原件新增第 3 個
        var command = new PasteCommand(target, snapshot);

        // Act
        command.Execute();

        // Assert — 貼上的副本反映快照時的 children 數量
        var pasted = (Directory)target.Children[0];
        pasted.Children.Should().HaveCount(2);
    }

    [Fact]
    public void Execute_DirectoryChildRemovedAfterSnapshot_PasteRetainsSnapshotChildren()
    {
        // Arrange
        var target = new Directory("Root");
        var dir = new Directory("Sub");
        var childA = new TextFile("a.txt", 100, TestDate, "UTF-8");
        dir.Add(childA);
        dir.Add(new TextFile("b.txt", 200, TestDate, "UTF-8"));
        var snapshot = dir.DeepCopy(); // 快照時有 2 children
        dir.Remove(childA); // 原件移除 1 個
        var command = new PasteCommand(target, snapshot);

        // Act
        command.Execute();

        // Assert — 貼上的副本仍保留快照時的 2 children
        var pasted = (Directory)target.Children[0];
        pasted.Children.Should().HaveCount(2);
    }
}
