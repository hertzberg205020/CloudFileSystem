using CloudFileSystem.ConsoleApp.Models;
using FluentAssertions;
using Xunit;
using Directory = CloudFileSystem.ConsoleApp.Models.Directory;

namespace CloudFileSystem.Tests.Models;

public class DeepCopyTests
{
    private static readonly DateTime TestDate = new(2025, 1, 1);

    [Fact]
    public void DeepCopy_TextFile_CreatesIndependentCopy()
    {
        var original = new TextFile("test.txt", 1024, TestDate, "UTF-8");

        var copy = (TextFile)original.DeepCopy();

        copy.Name.Should().Be("test.txt");
        copy.Size.Should().Be(1024);
        copy.Encoding.Should().Be("UTF-8");
        copy.Parent.Should().BeNull();
        copy.Should().NotBeSameAs(original);
    }

    [Fact]
    public void DeepCopy_WordDocument_CopiesAllProperties()
    {
        var original = new WordDocument("doc.docx", 512, TestDate, 10);

        var copy = (WordDocument)original.DeepCopy();

        copy.PageCount.Should().Be(10);
        copy.Should().NotBeSameAs(original);
    }

    [Fact]
    public void DeepCopy_ImageFile_CopiesAllProperties()
    {
        var original = new ImageFile("img.png", 2048, TestDate, 1920, 1080);

        var copy = (ImageFile)original.DeepCopy();

        copy.Width.Should().Be(1920);
        copy.Height.Should().Be(1080);
        copy.Should().NotBeSameAs(original);
    }

    [Fact]
    public void DeepCopy_Directory_RecursivelyCopiesChildren()
    {
        var root = new Directory("Root");
        var sub = new Directory("Sub");
        sub.Add(new TextFile("a.txt", 100, TestDate, "UTF-8"));
        root.Add(sub);
        root.Add(new WordDocument("b.docx", 200, TestDate, 5));

        var copy = (Directory)root.DeepCopy();

        copy.Name.Should().Be("Root");
        copy.Parent.Should().BeNull();
        copy.Children.Should().HaveCount(2);
        copy.Children[0].Should().NotBeSameAs(sub);
        copy.Children[0].Parent.Should().BeSameAs(copy);
        var subCopy = (Directory)copy.Children[0];
        subCopy.Children[0].Parent.Should().BeSameAs(subCopy);
    }

    [Fact]
    public void DeepCopy_PreservesTags()
    {
        var original = new TextFile("test.txt", 100, TestDate, "UTF-8");
        original.AddTag(Tag.Urgent);
        original.AddTag(Tag.Work);

        var copy = original.DeepCopy();

        copy.Tags.Should().BeEquivalentTo(new[] { Tag.Urgent, Tag.Work });
        original.RemoveTag(Tag.Urgent);
        copy.Tags.Should().Contain(Tag.Urgent, "副本的標籤應獨立於原始物件");
    }

    [Fact]
    public void DeepCopy_ModifyingCopy_DoesNotAffectOriginal()
    {
        var root = new Directory("Root");
        root.Add(new TextFile("a.txt", 100, TestDate, "UTF-8"));

        var copy = (Directory)root.DeepCopy();
        copy.Add(new TextFile("b.txt", 200, TestDate, "ASCII"));

        root.Children.Should().HaveCount(1);
        copy.Children.Should().HaveCount(2);
    }
}
