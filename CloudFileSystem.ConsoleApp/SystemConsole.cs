namespace CloudFileSystem.ConsoleApp;

/// <summary>
/// <see cref="IConsole"/> 的生產實作，委派至 <see cref="Console"/> 的對應方法。
/// </summary>
public class SystemConsole : IConsole
{
    public string? ReadLine() => Console.ReadLine();

    public void Write(string text) => Console.Write(text);

    public void WriteLine(string text) => Console.WriteLine(text);

    public void WriteError(string text) => Console.Error.WriteLine(text);
}
