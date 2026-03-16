using CloudFileSystem.ConsoleApp;
using CloudFileSystem.ConsoleApp.Models;

var root = new Directory("根目錄 (Root)");

var projectDocs = new Directory("專案文件 (Project_Docs)");
projectDocs.Add(new WordDocument("需求規格書.docx", 500 * 1024, new DateTime(2025, 1, 15), 15));
projectDocs.Add(
    new ImageFile("系統架構圖.png", 2 * 1024 * 1024, new DateTime(2025, 2, 1), 1920, 1080)
);

var personalNotes = new Directory("個人筆記 (Personal_Notes)");
personalNotes.Add(new TextFile("待辦清單.txt", 1024, new DateTime(2025, 3, 10), "UTF-8"));

var archive = new Directory("2025備份 (Archive_2025)");
archive.Add(new WordDocument("舊會議記錄.docx", 200 * 1024, new DateTime(2024, 8, 25), 5));
personalNotes.Add(archive);

root.Add(projectDocs);
root.Add(personalNotes);
root.Add(new TextFile("README.txt", 500, new DateTime(2025, 1, 1), "ASCII"));

var console = new SystemConsole();
var cli = new CloudFileSystemCli(root, console);
cli.Start();
