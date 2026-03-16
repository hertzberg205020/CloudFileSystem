namespace CloudFileSystem.ConsoleApp;

public interface IConsole
{
    string? ReadLine();
    void Write(string text);
    void WriteLine(string text);
    void WriteError(string text);
}
