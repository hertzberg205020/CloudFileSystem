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
}
