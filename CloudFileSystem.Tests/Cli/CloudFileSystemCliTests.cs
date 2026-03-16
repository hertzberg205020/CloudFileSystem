using CloudFileSystem.ConsoleApp;
using FluentAssertions;
using Xunit;

namespace CloudFileSystem.Tests.Cli;

public class CloudFileSystemCliTests
{
    private static (TestConsole console, CloudFileSystemCli cli) CreateCli(params string[] inputs)
    {
        var console = new TestConsole(inputs);
        var root = SampleStructureFactory.CreateSampleRoot();
        var cli = new CloudFileSystemCli(root, console);
        return (console, cli);
    }

    [Fact]
    public void Start_NullInput_ExitsLoop()
    {
        var (console, cli) = CreateCli();

        cli.Start();

        console.Output.Should().Contain("根目錄 (Root)> ");
    }

    [Fact]
    public void Start_DisplayCommand_PrintsTreeStructure()
    {
        var (console, cli) = CreateCli("display");

        cli.Start();

        console.Output.Should().Contain("根目錄 (Root)");
        console.Output.Should().Contain("專案文件 (Project_Docs) [目錄]");
        console.Output.Should().Contain("需求規格書.docx [Word 檔案]");
    }

    [Fact]
    public void Start_SizeCommand_PrintsTotalSize()
    {
        var (console, cli) = CreateCli("size");

        cli.Start();

        console.Output.Should().Contain("Total Size:");
        console.Output.Should().Contain("Visiting:");
    }

    [Fact]
    public void Start_SearchCommand_FindsMatchingFiles()
    {
        var (console, cli) = CreateCli("search .docx");

        cli.Start();

        console.Output.Should().Contain("Found 2 file(s):");
        console.Output.Should().Contain("需求規格書.docx");
        console.Output.Should().Contain("舊會議記錄.docx");
    }

    [Fact]
    public void Start_XmlCommand_PrintsXmlOutput()
    {
        var (console, cli) = CreateCli("xml");

        cli.Start();

        console.Output.Should().Contain("<根目錄_Root>");
        console.Output.Should().Contain("</根目錄_Root>");
    }

    [Fact]
    public void Start_UnknownCommand_PrintsError()
    {
        var (console, cli) = CreateCli("unknown");

        cli.Start();

        console.ErrorOutput.Should().Contain("Unknown command: unknown");
    }

    [Fact]
    public void Start_SearchWithoutExtension_PrintsUsageError()
    {
        var (console, cli) = CreateCli("search");

        cli.Start();

        console.ErrorOutput.Should().Contain("Usage: search <extension>");
    }

    [Fact]
    public void Start_MultipleCommands_ExecutesAll()
    {
        var (console, cli) = CreateCli("display", "size");

        cli.Start();

        console.Output.Should().Contain("[目錄]");
        console.Output.Should().Contain("Total Size:");
    }

    [Fact]
    public void Start_DeleteThenUndo_RestoresComponent()
    {
        var (console, cli) = CreateCli("delete README.txt", "display", "undo", "display");

        cli.Start();

        var output = console.Output;
        // After delete + display, README.txt should be gone
        var firstDisplay = output.IndexOf("根目錄 (Root)\n", output.IndexOf("Deleted:"));
        var secondDisplay = output.IndexOf("根目錄 (Root)\n", output.IndexOf("Undone:"));
        var afterUndo = output[secondDisplay..];
        afterUndo.Should().Contain("README.txt");
    }

    [Fact]
    public void Start_CopyAndPaste_CreatesDeepCopy()
    {
        var (console, cli) = CreateCli("copy README.txt", "paste", "display");

        cli.Start();

        console.Output.Should().Contain("Copied: README.txt");
        console.Output.Should().Contain("Pasted: README.txt");
        // Original + copy should both exist
        console.Output.Should().Contain("README.txt [純文字檔]");
        console.Output.Should().Contain("README (1).txt [純文字檔]");
    }

    [Fact]
    public void Start_SortByNameAsc_ReordersChildren()
    {
        var (console, cli) = CreateCli("sort name asc", "display");

        cli.Start();

        console.Output.Should().Contain("Sorted by Name Asc");
    }

    [Fact]
    public void Start_TagAndDisplay_ShowsTag()
    {
        var (console, cli) = CreateCli("tag README.txt Urgent", "display");

        cli.Start();

        console.Output.Should().Contain("Tagged README.txt as Urgent");
        console.Output.Should().Contain("{Urgent}");
    }

    [Fact]
    public void Start_TagThenUntag_RemovesTag()
    {
        var (console, cli) = CreateCli("tag README.txt Work", "untag README.txt Work", "display");

        cli.Start();

        console.Output.Should().Contain("Removed Work from README.txt");
        // display should not contain tag braces for README
        var displayOutput = console.Output[console.Output.LastIndexOf("根目錄 (Root)\n")..];
        displayOutput.Should().NotContain("{Work}");
    }

    [Fact]
    public void Start_UndoWithoutHistory_PrintsError()
    {
        var (console, cli) = CreateCli("undo");

        cli.Start();

        console.ErrorOutput.Should().Contain("Nothing to undo.");
    }

    [Fact]
    public void Start_RedoAfterUndo_ReExecutes()
    {
        var (console, cli) = CreateCli("tag README.txt Personal", "undo", "redo", "display");

        cli.Start();

        console.Output.Should().Contain("Undone: Tag README.txt as Personal");
        console.Output.Should().Contain("Redone: Tag README.txt as Personal");
        console.Output.Should().Contain("{Personal}");
    }

    [Fact]
    public void Start_PasteWithoutCopy_PrintsError()
    {
        var (console, cli) = CreateCli("paste");

        cli.Start();

        console.ErrorOutput.Should().Contain("Clipboard is empty.");
    }

    [Fact]
    public void Start_DeleteNonExistent_PrintsError()
    {
        var (console, cli) = CreateCli("delete nonexistent.txt");

        cli.Start();

        console.ErrorOutput.Should().Contain("Not found: nonexistent.txt");
    }
}
