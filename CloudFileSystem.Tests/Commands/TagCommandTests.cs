using CloudFileSystem.ConsoleApp.Commands;
using CloudFileSystem.ConsoleApp.Models;
using FluentAssertions;
using Xunit;

namespace CloudFileSystem.Tests.Commands;

public class TagCommandTests
{
    [Fact]
    public void TagCommand_Execute_AddsTag()
    {
        var file = new TextFile("test.txt", 100, DateTime.Now, "UTF-8");
        var command = new TagCommand(file, Tag.Urgent);

        command.Execute();

        file.Tags.Should().Contain(Tag.Urgent);
    }

    [Fact]
    public void TagCommand_Undo_RemovesTag()
    {
        var file = new TextFile("test.txt", 100, DateTime.Now, "UTF-8");
        var command = new TagCommand(file, Tag.Urgent);
        command.Execute();

        command.Undo();

        file.Tags.Should().NotContain(Tag.Urgent);
    }

    [Fact]
    public void UntagCommand_Execute_RemovesTag()
    {
        var file = new TextFile("test.txt", 100, DateTime.Now, "UTF-8");
        file.AddTag(Tag.Work);
        var command = new UntagCommand(file, Tag.Work);

        command.Execute();

        file.Tags.Should().NotContain(Tag.Work);
    }

    [Fact]
    public void UntagCommand_Undo_RestoresTag()
    {
        var file = new TextFile("test.txt", 100, DateTime.Now, "UTF-8");
        file.AddTag(Tag.Work);
        var command = new UntagCommand(file, Tag.Work);
        command.Execute();

        command.Undo();

        file.Tags.Should().Contain(Tag.Work);
    }
}
