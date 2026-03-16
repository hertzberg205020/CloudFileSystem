using CloudFileSystem.ConsoleApp.Models;
using FluentAssertions;
using Xunit;

namespace CloudFileSystem.Tests.Models;

public class TagTests
{
    [Fact]
    public void AddTag_NewTag_AddsToCollection()
    {
        var file = new TextFile("test.txt", 100, DateTime.Now, "UTF-8");

        file.AddTag(Tag.Urgent);

        file.Tags.Should().Contain(Tag.Urgent);
    }

    [Fact]
    public void AddTag_DuplicateTag_DoesNotDuplicate()
    {
        var file = new TextFile("test.txt", 100, DateTime.Now, "UTF-8");

        file.AddTag(Tag.Work);
        file.AddTag(Tag.Work);

        file.Tags.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveTag_ExistingTag_RemovesFromCollection()
    {
        var file = new TextFile("test.txt", 100, DateTime.Now, "UTF-8");
        file.AddTag(Tag.Personal);

        file.RemoveTag(Tag.Personal);

        file.Tags.Should().BeEmpty();
    }

    [Fact]
    public void Tags_NewComponent_IsEmpty()
    {
        var file = new TextFile("test.txt", 100, DateTime.Now, "UTF-8");

        file.Tags.Should().BeEmpty();
    }

    [Fact]
    public void AddTag_MultipleTags_SupportsMultiple()
    {
        var dir = new CloudFileSystem.ConsoleApp.Models.Directory("TestDir");

        dir.AddTag(Tag.Urgent);
        dir.AddTag(Tag.Work);
        dir.AddTag(Tag.Personal);

        dir.Tags.Should().HaveCount(3);
    }
}
