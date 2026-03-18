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

    [Fact]
    public void Start_TagNestedFile_TagsViaPath()
    {
        var (console, cli) = CreateCli(
            "tag 個人筆記 (Personal_Notes)/待辦清單.txt Urgent",
            "display"
        );

        cli.Start();

        console.Output.Should().Contain("Tagged 待辦清單.txt as Urgent");
        console
            .Output.Should()
            .Contain("待辦清單.txt [純文字檔] (編碼: UTF-8, 大小: 1KB) {Urgent}");
    }

    [Fact]
    public void Start_TagDeeplyNestedFile_TagsViaPath()
    {
        var (console, cli) = CreateCli(
            "tag 個人筆記 (Personal_Notes)/2025備份 (Archive_2025)/舊會議記錄.docx Work",
            "display"
        );

        cli.Start();

        console.Output.Should().Contain("Tagged 舊會議記錄.docx as Work");
        console.Output.Should().Contain("{Work}");
    }

    [Fact]
    public void Start_DeleteNestedFile_RemovesViaPath()
    {
        var (console, cli) = CreateCli("delete 個人筆記 (Personal_Notes)/待辦清單.txt", "display");

        cli.Start();

        console.Output.Should().Contain("Deleted: 待辦清單.txt");
        // display 後不應再包含待辦清單.txt
        var displayOutput = console.Output[console.Output.IndexOf("根目錄 (Root)\n")..];
        displayOutput.Should().NotContain("待辦清單.txt");
    }

    [Fact]
    public void Start_CopyNestedFile_CopiesViaPath()
    {
        var (console, cli) = CreateCli(
            "copy 個人筆記 (Personal_Notes)/待辦清單.txt",
            "paste",
            "display"
        );

        cli.Start();

        console.Output.Should().Contain("Copied: 待辦清單.txt");
        console.Output.Should().Contain("Pasted: 待辦清單.txt");
    }

    [Fact]
    public void Start_TagNestedDirectory_TagsViaPath()
    {
        var (console, cli) = CreateCli(
            "tag 個人筆記 (Personal_Notes)/2025備份 (Archive_2025) Personal",
            "display"
        );

        cli.Start();

        console.Output.Should().Contain("Tagged 2025備份 (Archive_2025) as Personal");
        console.Output.Should().Contain("{Personal}");
    }

    [Fact]
    public void Start_InvalidPath_PrintsNotFound()
    {
        var (console, cli) = CreateCli("tag 不存在的目錄/file.txt Urgent");

        cli.Start();

        console.ErrorOutput.Should().Contain("Not found:");
    }

    [Fact]
    public void Start_PasteToSpecificDirectory_PastesIntoTarget()
    {
        var (console, cli) = CreateCli(
            "copy README.txt",
            "paste 專案文件 (Project_Docs)",
            "display"
        );

        cli.Start();

        console.Output.Should().Contain("Pasted: README.txt");
        // README.txt 應出現在專案文件底下（作為其子元件）
        var display = console.Output[console.Output.LastIndexOf("根目錄 (Root)\n")..];
        var lines = display.Split('\n');
        var projectDocsIndex = Array.FindIndex(lines, l => l.Contains("專案文件 (Project_Docs)"));
        var readmeUnderProject = Array.FindIndex(
            lines,
            projectDocsIndex + 1,
            l => l.Contains("README.txt") && l.Contains("│")
        );
        readmeUnderProject.Should().BeGreaterThan(projectDocsIndex);
    }

    [Fact]
    public void Start_PasteToNestedDirectory_PastesViaPath()
    {
        var (console, cli) = CreateCli(
            "copy README.txt",
            "paste 個人筆記 (Personal_Notes)/2025備份 (Archive_2025)",
            "display"
        );

        cli.Start();

        console.Output.Should().Contain("Pasted: README.txt");
    }

    [Fact]
    public void Start_PasteWithoutPath_PastesToRoot()
    {
        var (console, cli) = CreateCli("copy README.txt", "paste", "display");

        cli.Start();

        // 向下相容：無路徑時貼到根目錄
        console.Output.Should().Contain("README (1).txt");
    }

    [Fact]
    public void Start_PasteToNonDirectory_PrintsError()
    {
        var (console, cli) = CreateCli("copy README.txt", "paste README.txt");

        cli.Start();

        console.ErrorOutput.Should().Contain("Not a directory:");
    }

    [Fact]
    public void Start_PasteToInvalidPath_PrintsError()
    {
        var (console, cli) = CreateCli("copy README.txt", "paste 不存在的目錄");

        cli.Start();

        console.ErrorOutput.Should().Contain("Not found:");
    }

    [Fact]
    public void Start_DeleteSameComponentTwice_ShowsErrorAndContinues()
    {
        var (console, cli) = CreateCli("delete README.txt", "delete README.txt", "display");

        cli.Start();

        console.Output.Should().Contain("Deleted: README.txt");
        console.ErrorOutput.Should().Contain("Not found: README.txt");
        // display 仍正常執行
        console.Output.Should().Contain("根目錄 (Root)");
    }

    [Fact]
    public void Start_ExceptionDuringCommand_LoopContinues()
    {
        // 先刪除再 undo，再重複刪除觸發例外，最後 display 確認迴圈繼續
        var (console, cli) = CreateCli(
            "delete README.txt",
            "undo",
            "delete README.txt",
            "delete README.txt",
            "display"
        );

        cli.Start();

        console.ErrorOutput.Should().Contain("Not found: README.txt");
        // display 仍正常執行（找 display tree 輸出而非 prompt）
        console.Output.Should().Contain("專案文件 (Project_Docs) [目錄]");
    }

    [Fact]
    public void Start_TagWithQuotedName_TagsCorrectly()
    {
        var (console, cli) = CreateCli(
            "tag \"個人筆記 (Personal_Notes)/待辦清單.txt\" Urgent",
            "display"
        );

        cli.Start();

        console.Output.Should().Contain("Tagged 待辦清單.txt as Urgent");
        console
            .Output.Should()
            .Contain("待辦清單.txt [純文字檔] (編碼: UTF-8, 大小: 1KB) {Urgent}");
    }

    [Fact]
    public void Start_UntagWithQuotedName_UntagsCorrectly()
    {
        var (console, cli) = CreateCli(
            "tag \"個人筆記 (Personal_Notes)/待辦清單.txt\" Work",
            "untag \"個人筆記 (Personal_Notes)/待辦清單.txt\" Work",
            "display"
        );

        cli.Start();

        console.Output.Should().Contain("Removed Work from 待辦清單.txt");
        var displayOutput = console.Output[console.Output.LastIndexOf("根目錄 (Root)\n")..];
        displayOutput.Should().NotContain("{Work}");
    }

    [Fact]
    public void Start_SortInvalidField_PrintsError()
    {
        var (console, cli) = CreateCli("sort invalid asc");

        cli.Start();

        console.ErrorOutput.Should().Contain("Invalid sort field");
    }

    [Fact]
    public void Start_SortInvalidOrder_PrintsError()
    {
        var (console, cli) = CreateCli("sort name invalid");

        cli.Start();

        console.ErrorOutput.Should().Contain("Invalid sort order");
    }

    [Fact]
    public void Start_TagWithoutEnoughArgs_PrintsUsage()
    {
        var (console, cli) = CreateCli("tag README.txt");

        cli.Start();

        console.ErrorOutput.Should().Contain("Usage: tag");
    }

    [Fact]
    public void Start_TagInvalidTagValue_PrintsError()
    {
        var (console, cli) = CreateCli("tag README.txt InvalidTag");

        cli.Start();

        console.ErrorOutput.Should().Contain("Invalid tag");
    }

    [Fact]
    public void Start_UntagWithoutEnoughArgs_PrintsUsage()
    {
        var (console, cli) = CreateCli("untag README.txt");

        cli.Start();

        console.ErrorOutput.Should().Contain("Usage: untag");
    }

    [Fact]
    public void Start_CopyWithoutArgs_PrintsUsage()
    {
        var (console, cli) = CreateCli("copy");

        cli.Start();

        console.ErrorOutput.Should().Contain("Usage: copy");
    }

    [Fact]
    public void Start_DeleteWithoutArgs_PrintsUsage()
    {
        var (console, cli) = CreateCli("delete");

        cli.Start();

        console.ErrorOutput.Should().Contain("Usage: delete");
    }

    // === size 指令路徑參數測試 ===

    [Fact]
    public void Start_SizeNoArgs_StillPrintsTotalSize()
    {
        var (console, cli) = CreateCli("size");

        cli.Start();

        console.Output.Should().Contain("Total Size:");
    }

    [Fact]
    public void Start_SizeWithPath_PrintsSubdirectorySize()
    {
        var (console, cli) = CreateCli("size 專案文件 (Project_Docs)");

        cli.Start();

        console.Output.Should().Contain("Total Size: 2.5MB");
    }

    [Fact]
    public void Start_SizeWithNestedPath_PrintsNestedDirectorySize()
    {
        var (console, cli) = CreateCli(
            "size 個人筆記 (Personal_Notes)/2025備份 (Archive_2025)"
        );

        cli.Start();

        console.Output.Should().Contain("Total Size: 200KB");
    }

    [Fact]
    public void Start_SizeWithNonExistentPath_PrintsError()
    {
        var (console, cli) = CreateCli("size 不存在的目錄");

        cli.Start();

        console.ErrorOutput.Should().Contain("Not found:");
    }

    [Fact]
    public void Start_SizeWithFilePath_PrintsError()
    {
        var (console, cli) = CreateCli("size README.txt");

        cli.Start();

        console.ErrorOutput.Should().Contain("Not a directory:");
    }

    [Fact]
    public void Start_SizeWithEmptyDirectory_PrintsZero()
    {
        var (console, cli) = CreateCli(
            "delete 個人筆記 (Personal_Notes)/2025備份 (Archive_2025)/舊會議記錄.docx",
            "size 個人筆記 (Personal_Notes)/2025備份 (Archive_2025)"
        );

        cli.Start();

        console.Output.Should().Contain("Total Size: 0B");
    }
}
