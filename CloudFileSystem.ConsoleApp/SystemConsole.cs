namespace CloudFileSystem.ConsoleApp;

public class SystemConsole : IConsole
{
    public string? ReadLine() => Console.ReadLine();

    public void Write(string text) => Console.Write(text);

    public void WriteLine(string text) => Console.WriteLine(text);

    public void WriteError(string text) => Console.Error.WriteLine(text);
}
