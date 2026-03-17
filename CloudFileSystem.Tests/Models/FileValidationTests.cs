using CloudFileSystem.ConsoleApp.Models;
using FluentAssertions;
using Xunit;

namespace CloudFileSystem.Tests.Models;

public class FileValidationTests
{
    private static readonly DateTime TestDate = new(2025, 1, 1);

    [Fact]
    public void File_NegativeSize_ThrowsArgumentOutOfRangeException()
    {
        var act = () => new TextFile("test.txt", -1, TestDate, "UTF-8");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void File_ZeroSize_Succeeds()
    {
        var file = new TextFile("test.txt", 0, TestDate, "UTF-8");

        file.Size.Should().Be(0);
    }

    [Fact]
    public void WordDocument_NegativePageCount_ThrowsArgumentOutOfRangeException()
    {
        var act = () => new WordDocument("doc.docx", 1024, TestDate, -1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void WordDocument_ZeroPageCount_Succeeds()
    {
        var doc = new WordDocument("doc.docx", 1024, TestDate, 0);

        doc.PageCount.Should().Be(0);
    }

    [Fact]
    public void ImageFile_ZeroWidth_ThrowsArgumentOutOfRangeException()
    {
        var act = () => new ImageFile("img.png", 1024, TestDate, 0, 100);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ImageFile_ZeroHeight_ThrowsArgumentOutOfRangeException()
    {
        var act = () => new ImageFile("img.png", 1024, TestDate, 100, 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ImageFile_NegativeWidth_ThrowsArgumentOutOfRangeException()
    {
        var act = () => new ImageFile("img.png", 1024, TestDate, -1, 100);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ImageFile_NegativeHeight_ThrowsArgumentOutOfRangeException()
    {
        var act = () => new ImageFile("img.png", 1024, TestDate, 100, -1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TextFile_NullEncoding_ThrowsArgumentException()
    {
        var act = () => new TextFile("test.txt", 1024, TestDate, null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TextFile_EmptyEncoding_ThrowsArgumentException()
    {
        var act = () => new TextFile("test.txt", 1024, TestDate, "");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TextFile_WhitespaceEncoding_ThrowsArgumentException()
    {
        var act = () => new TextFile("test.txt", 1024, TestDate, "   ");

        act.Should().Throw<ArgumentException>();
    }
}
