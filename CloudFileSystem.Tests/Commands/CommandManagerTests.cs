using CloudFileSystem.ConsoleApp.Commands;
using FluentAssertions;
using Xunit;

namespace CloudFileSystem.Tests.Commands;

public class CommandManagerTests
{
    private readonly CommandManager _manager = new();

    [Fact]
    public void Execute_SingleCommand_CanUndo()
    {
        var command = new FakeCommand("add item");

        _manager.Execute(command);

        _manager.CanUndo.Should().BeTrue();
        command.ExecuteCount.Should().Be(1);
    }

    [Fact]
    public void Undo_AfterExecute_ReversesCommand()
    {
        var command = new FakeCommand("add item");
        _manager.Execute(command);

        var undone = _manager.Undo();

        undone.Should().BeSameAs(command);
        command.UndoCount.Should().Be(1);
        _manager.CanUndo.Should().BeFalse();
        _manager.CanRedo.Should().BeTrue();
    }

    [Fact]
    public void Redo_AfterUndo_ReExecutesCommand()
    {
        var command = new FakeCommand("add item");
        _manager.Execute(command);
        _manager.Undo();

        var redone = _manager.Redo();

        redone.Should().BeSameAs(command);
        command.ExecuteCount.Should().Be(2);
        _manager.CanUndo.Should().BeTrue();
        _manager.CanRedo.Should().BeFalse();
    }

    [Fact]
    public void Execute_AfterUndo_ClearsRedoStack()
    {
        var first = new FakeCommand("first");
        var second = new FakeCommand("second");
        _manager.Execute(first);
        _manager.Undo();

        _manager.Execute(second);

        _manager.CanRedo.Should().BeFalse();
        _manager.CanUndo.Should().BeTrue();
    }

    [Fact]
    public void Undo_WhenEmpty_ReturnsNull()
    {
        var result = _manager.Undo();

        result.Should().BeNull();
    }

    [Fact]
    public void Redo_WhenEmpty_ReturnsNull()
    {
        var result = _manager.Redo();

        result.Should().BeNull();
    }

    [Fact]
    public void MultipleUndoRedo_MaintainsCorrectOrder()
    {
        var first = new FakeCommand("first");
        var second = new FakeCommand("second");
        var third = new FakeCommand("third");
        _manager.Execute(first);
        _manager.Execute(second);
        _manager.Execute(third);

        _manager.Undo().Should().BeSameAs(third);
        _manager.Undo().Should().BeSameAs(second);
        _manager.Redo().Should().BeSameAs(second);
        _manager.Redo().Should().BeSameAs(third);
        _manager.Undo().Should().BeSameAs(third);
    }

    [Fact]
    public void Undo_UndoThrows_CommandRemainsOnUndoStack()
    {
        var command = new ThrowingCommand();
        _manager.Execute(command);
        command.ShouldThrowOnUndo = true;

        var act = () => _manager.Undo();

        act.Should().Throw<InvalidOperationException>().WithMessage("Undo failed");
        _manager.CanUndo.Should().BeTrue();
        _manager.CanRedo.Should().BeFalse();
    }

    [Fact]
    public void Redo_ExecuteThrows_CommandRemainsOnRedoStack()
    {
        var command = new ThrowingCommand();
        _manager.Execute(command);
        _manager.Undo();
        command.ShouldThrowOnExecute = true;

        var act = () => _manager.Redo();

        act.Should().Throw<InvalidOperationException>().WithMessage("Execute failed");
        _manager.CanRedo.Should().BeTrue();
        _manager.CanUndo.Should().BeFalse();
    }

    /// <summary>
    /// 用於測試的假 Command，記錄 Execute/Undo 呼叫次數。
    /// </summary>
    private sealed class FakeCommand : ICommand
    {
        public int ExecuteCount { get; private set; }
        public int UndoCount { get; private set; }
        public string Description { get; }

        public FakeCommand(string description)
        {
            Description = description;
        }

        public void Execute() => ExecuteCount++;

        public void Undo() => UndoCount++;
    }

    private sealed class ThrowingCommand : ICommand
    {
        public bool ShouldThrowOnExecute { get; set; }
        public bool ShouldThrowOnUndo { get; set; }
        public string Description => "throwing";

        public void Execute()
        {
            if (ShouldThrowOnExecute)
                throw new InvalidOperationException("Execute failed");
        }

        public void Undo()
        {
            if (ShouldThrowOnUndo)
                throw new InvalidOperationException("Undo failed");
        }
    }
}
