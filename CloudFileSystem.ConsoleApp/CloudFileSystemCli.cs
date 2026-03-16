using CloudFileSystem.ConsoleApp.Models;
using CloudFileSystem.ConsoleApp.Visitors;

namespace CloudFileSystem.ConsoleApp;

public class CloudFileSystemCli
{
    private readonly IConsole _console;
    private readonly Directory _root;

    public CloudFileSystemCli(Directory root, IConsole console)
    {
        _root = root;
        _console = console;
    }

    public void Start()
    {
        while (true)
        {
            _console.Write($"{_root.Name}> ");
            var input = _console.ReadLine();
            if (input == null)
                break;

            var command = input.Trim();
            if (string.IsNullOrEmpty(command))
                continue;

            ExecuteCommand(command);
        }
    }

    private void ExecuteCommand(string command)
    {
        var parts = command.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var action = parts[0].ToLowerInvariant();

        switch (action)
        {
            case "display":
                var displayVisitor = new DisplayVisitor(_console);
                _root.Accept(displayVisitor);
                break;

            case "size":
                var sizeVisitor = new SizeCalculatorVisitor(_console);
                _root.Accept(sizeVisitor);
                _console.WriteLine($"Total Size: {File.FormatSize(sizeVisitor.TotalSize)}");
                break;

            case "search":
                if (parts.Length < 2)
                {
                    _console.WriteError("Usage: search <extension>");
                    break;
                }

                var searchVisitor = new SearchByExtensionVisitor(parts[1], _console);
                _root.Accept(searchVisitor);
                _console.WriteLine($"Found {searchVisitor.Results.Count} file(s):");
                foreach (var result in searchVisitor.Results)
                    _console.WriteLine($"  {result}");
                break;

            case "xml":
                var xmlVisitor = new XmlExportVisitor();
                _root.Accept(xmlVisitor);
                _console.WriteLine(xmlVisitor.GetXml().TrimEnd());
                break;

            case "exit":
                return;

            default:
                _console.WriteError($"Unknown command: {action}");
                break;
        }
    }
}
