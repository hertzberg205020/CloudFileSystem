using CloudFileSystem.ConsoleApp.Models;
using Directory = CloudFileSystem.ConsoleApp.Models.Directory;

namespace CloudFileSystem.Tests;

public static class SampleStructureFactory
{
    private static readonly DateTime TestDate = new(2025, 1, 1);

    public static Directory CreateSampleRoot()
    {
        var root = new Directory("根目錄 (Root)");

        var projectDocs = new Directory("專案文件 (Project_Docs)");
        projectDocs.Add(new WordDocument("需求規格書.docx", 500 * 1024, TestDate, 15));
        projectDocs.Add(new ImageFile("系統架構圖.png", 2 * 1024 * 1024, TestDate, 1920, 1080));

        var personalNotes = new Directory("個人筆記 (Personal_Notes)");
        personalNotes.Add(new TextFile("待辦清單.txt", 1024, TestDate, "UTF-8"));

        var archive = new Directory("2025備份 (Archive_2025)");
        archive.Add(new WordDocument("舊會議記錄.docx", 200 * 1024, TestDate, 5));
        personalNotes.Add(archive);

        root.Add(projectDocs);
        root.Add(personalNotes);
        root.Add(new TextFile("README.txt", 500, TestDate, "ASCII"));

        return root;
    }
}
