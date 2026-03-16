using System.Text;
using CloudFileSystem.ConsoleApp;

namespace CloudFileSystem.Tests;

public class TestConsole : IConsole
{
    private readonly Queue<string> _inputs;
    private readonly StringBuilder _output = new();
    private readonly StringBuilder _errorOutput = new();

    public string Output => _output.ToString();
    public string ErrorOutput => _errorOutput.ToString();

    public TestConsole(params string[] inputs)
    {
        _inputs = new Queue<string>(inputs);
    }

    public string? ReadLine() => _inputs.Count > 0 ? _inputs.Dequeue() : null;

    public void Write(string text) => _output.Append(text);

    public void WriteLine(string text) => _output.AppendLine(text);

    public void WriteError(string text) => _errorOutput.AppendLine(text);
}
