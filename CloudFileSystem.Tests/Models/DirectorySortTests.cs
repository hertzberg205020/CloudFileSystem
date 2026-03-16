using CloudFileSystem.ConsoleApp.Models;
using FluentAssertions;
using Xunit;
using Directory = CloudFileSystem.ConsoleApp.Models.Directory;

namespace CloudFileSystem.Tests.Models;

public class DirectorySortTests
{
    private static readonly DateTime TestDate = new(2025, 1, 1);

    [Fact]
    public void Sort_ByNameAsc_SortsAlphabetically()
    {
        var dir = new Directory("Root");
        dir.Add(new TextFile("charlie.txt", 100, TestDate, "UTF-8"));
        dir.Add(new TextFile("alpha.txt", 200, TestDate, "UTF-8"));
        dir.Add(new TextFile("bravo.txt", 300, TestDate, "UTF-8"));

        dir.Sort(SortBy.Name, SortOrder.Asc);

        dir.Children.Select(c => c.Name)
            .Should()
            .ContainInOrder("alpha.txt", "bravo.txt", "charlie.txt");
    }

    [Fact]
    public void Sort_ByNameDesc_SortsReverseAlphabetically()
    {
        var dir = new Directory("Root");
        dir.Add(new TextFile("alpha.txt", 100, TestDate, "UTF-8"));
        dir.Add(new TextFile("charlie.txt", 200, TestDate, "UTF-8"));

        dir.Sort(SortBy.Name, SortOrder.Desc);

        dir.Children.Select(c => c.Name)
            .Should()
            .ContainInOrder("charlie.txt", "alpha.txt");
    }

    [Fact]
    public void Sort_BySizeAsc_SortsByFileSize()
    {
        var dir = new Directory("Root");
        dir.Add(new TextFile("big.txt", 3000, TestDate, "UTF-8"));
        dir.Add(new TextFile("small.txt", 100, TestDate, "UTF-8"));
        dir.Add(new TextFile("medium.txt", 1500, TestDate, "UTF-8"));

        dir.Sort(SortBy.Size, SortOrder.Asc);

        dir.Children.Select(c => c.Name)
            .Should()
            .ContainInOrder("small.txt", "medium.txt", "big.txt");
    }

    [Fact]
    public void Sort_ByExtensionAsc_SortsByFileExtension()
    {
        var dir = new Directory("Root");
        dir.Add(new TextFile("file.txt", 100, TestDate, "UTF-8"));
        dir.Add(new WordDocument("doc.docx", 200, TestDate, 5));
        dir.Add(new ImageFile("img.png", 300, TestDate, 800, 600));

        dir.Sort(SortBy.Extension, SortOrder.Asc);

        dir.Children.Select(c => c.Name)
            .Should()
            .ContainInOrder("doc.docx", "img.png", "file.txt");
    }

    [Fact]
    public void Insert_AtIndex_InsertsAtCorrectPosition()
    {
        var dir = new Directory("Root");
        var first = new TextFile("first.txt", 100, TestDate, "UTF-8");
        var third = new TextFile("third.txt", 300, TestDate, "UTF-8");
        dir.Add(first);
        dir.Add(third);

        var second = new TextFile("second.txt", 200, TestDate, "UTF-8");
        dir.Insert(1, second);

        dir.Children[1].Should().BeSameAs(second);
        second.Parent.Should().BeSameAs(dir);
    }

    [Fact]
    public void IndexOf_ExistingChild_ReturnsCorrectIndex()
    {
        var dir = new Directory("Root");
        var file = new TextFile("test.txt", 100, TestDate, "UTF-8");
        dir.Add(new TextFile("other.txt", 50, TestDate, "UTF-8"));
        dir.Add(file);

        dir.IndexOf(file).Should().Be(1);
    }

    [Fact]
    public void Sort_ByExtensionAsc_RecursivelySortsSubDirectories()
    {
        var root = new Directory("Root");
        var sub = new Directory("Sub");
        sub.Add(new TextFile("z.txt", 100, TestDate, "UTF-8"));
        sub.Add(new WordDocument("a.docx", 200, TestDate, 5));
        root.Add(sub);
        root.Add(new TextFile("file.txt", 300, TestDate, "UTF-8"));

        root.Sort(SortBy.Extension, SortOrder.Asc);

        // 子目錄（ext ""）在前，.txt 在後
        root.Children[0].Should().BeSameAs(sub);
        // 子目錄內部也應遞迴排序：.docx 在前、.txt 在後
        sub.Children.Select(c => c.Name)
            .Should()
            .ContainInOrder("a.docx", "z.txt");
    }

    [Fact]
    public void Sort_ByNameAsc_RecursivelySortsSubDirectories()
    {
        var root = new Directory("Root");
        var sub = new Directory("Sub");
        sub.Add(new TextFile("charlie.txt", 100, TestDate, "UTF-8"));
        sub.Add(new TextFile("alpha.txt", 200, TestDate, "UTF-8"));
        root.Add(sub);

        root.Sort(SortBy.Name, SortOrder.Asc);

        sub.Children.Select(c => c.Name)
            .Should()
            .ContainInOrder("alpha.txt", "charlie.txt");
    }

    [Fact]
    public void Sort_BySizeDesc_RecursivelySortsSubDirectories()
    {
        var root = new Directory("Root");
        var sub = new Directory("Sub");
        sub.Add(new TextFile("small.txt", 100, TestDate, "UTF-8"));
        sub.Add(new TextFile("big.txt", 3000, TestDate, "UTF-8"));
        root.Add(sub);

        root.Sort(SortBy.Size, SortOrder.Desc);

        sub.Children.Select(c => c.Name)
            .Should()
            .ContainInOrder("big.txt", "small.txt");
    }

    [Fact]
    public void SetChildrenOrder_RestoresOriginalOrder()
    {
        var dir = new Directory("Root");
        var a = new TextFile("a.txt", 100, TestDate, "UTF-8");
        var b = new TextFile("b.txt", 200, TestDate, "UTF-8");
        var c = new TextFile("c.txt", 300, TestDate, "UTF-8");
        dir.Add(a);
        dir.Add(b);
        dir.Add(c);
        var originalOrder = dir.Children.ToList();

        dir.Sort(SortBy.Size, SortOrder.Desc);
        dir.SetChildrenOrder(originalOrder);

        dir.Children.Select(ch => ch.Name)
            .Should()
            .ContainInOrder("a.txt", "b.txt", "c.txt");
    }
}
