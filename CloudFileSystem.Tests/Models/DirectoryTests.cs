using CloudFileSystem.ConsoleApp.Models;
using FluentAssertions;
using Xunit;
using Directory = CloudFileSystem.ConsoleApp.Models.Directory;
using File = CloudFileSystem.ConsoleApp.Models.File;

namespace CloudFileSystem.Tests.Models;

public class DirectoryTests
{
    private static readonly DateTime TestDate = new(2025, 1, 1);

    [Fact]
    public void GetSize_EmptyDirectory_ReturnsZero()
    {
        var dir = new Directory("Empty");

        dir.GetSize().Should().Be(0);
    }

    [Fact]
    public void GetSize_WithFiles_ReturnsSumOfFileSizes()
    {
        var dir = new Directory("Docs");
        dir.Add(new TextFile("a.txt", 1024, TestDate, "UTF-8"));
        dir.Add(new TextFile("b.txt", 2048, TestDate, "UTF-8"));

        dir.GetSize().Should().Be(3072);
    }

    [Fact]
    public void GetSize_WithNestedDirectories_ReturnsTotalRecursively()
    {
        var root = new Directory("Root");
        var sub = new Directory("Sub");
        root.Add(sub);
        sub.Add(new WordDocument("doc.docx", 500 * 1024, TestDate, 10));
        root.Add(new TextFile("readme.txt", 500, TestDate, "ASCII"));

        root.GetSize().Should().Be(500 * 1024 + 500);
    }

    [Fact]
    public void Add_SetsParentCorrectly()
    {
        var root = new Directory("Root");
        var child = new Directory("Child");

        root.Add(child);

        child.Parent.Should().Be(root);
        root.Children.Should().Contain(child);
    }

    [Fact]
    public void Remove_ClearsParentAndRemovesChild()
    {
        var root = new Directory("Root");
        var child = new Directory("Child");
        root.Add(child);

        root.Remove(child);

        child.Parent.Should().BeNull();
        root.Children.Should().NotContain(child);
    }

    [Fact]
    public void GetPath_RootDirectory_ReturnsName()
    {
        var root = new Directory("根目錄 (Root)");

        root.GetPath().Should().Be("根目錄 (Root)");
    }

    [Fact]
    public void GetPath_NestedComponent_ReturnsFullPath()
    {
        var root = new Directory("Root");
        var sub = new Directory("Sub");
        var file = new TextFile("file.txt", 100, TestDate, "UTF-8");
        root.Add(sub);
        sub.Add(file);

        file.GetPath().Should().Be("Root/Sub/file.txt");
    }
}

public class FileTests
{
    private static readonly DateTime TestDate = new(2025, 1, 1);

    [Fact]
    public void WordDocument_GetSize_ReturnsSize()
    {
        var doc = new WordDocument("test.docx", 512000, TestDate, 15);

        doc.GetSize().Should().Be(512000);
        doc.PageCount.Should().Be(15);
    }

    [Fact]
    public void ImageFile_StoresResolution()
    {
        var img = new ImageFile("photo.png", 2 * 1024 * 1024, TestDate, 1920, 1080);

        img.Width.Should().Be(1920);
        img.Height.Should().Be(1080);
        img.GetSize().Should().Be(2 * 1024 * 1024);
    }

    [Fact]
    public void TextFile_StoresEncoding()
    {
        var txt = new TextFile("notes.txt", 1024, TestDate, "UTF-8");

        txt.Encoding.Should().Be("UTF-8");
    }

    [Theory]
    [InlineData(500, "500B")]
    [InlineData(1023, "1023B")]
    [InlineData(1024, "1KB")]
    [InlineData(1025, "1KB")]
    [InlineData(1536, "1.5KB")]
    [InlineData(500 * 1024, "500KB")]
    [InlineData(1048575, "1024KB")]
    [InlineData(1048576, "1MB")]
    [InlineData(1572864, "1.5MB")]
    [InlineData(2 * 1024 * 1024, "2MB")]
    public void FormatSize_VariousValues_ReturnsExpectedFormat(long bytes, string expected)
    {
        File.FormatSize(bytes).Should().Be(expected);
    }
}
