using CloudFileSystem.ConsoleApp.Models;

namespace CloudFileSystem.ConsoleApp.Visitors;

/// <summary>
/// 遞迴計算目錄總容量的 Visitor，對應功能二-1。
/// </summary>
/// <remarks>
/// 走訪過程中會透過 <see cref="IConsole"/> 輸出 Traverse Log，
/// 結果可透過 <see cref="TotalSize"/> 屬性取得。
/// </remarks>
public class SizeCalculatorVisitor : IFileSystemVisitor
{
    private readonly IConsole _console;

    /// <summary>
    /// 取得走訪後計算的總容量（單位：Bytes）。
    /// </summary>
    public long TotalSize { get; private set; }

    /// <summary>
    /// 初始化 <see cref="SizeCalculatorVisitor"/> 的新執行個體。
    /// </summary>
    /// <param name="console">用於輸出 Traverse Log 的 Console 抽象。</param>
    public SizeCalculatorVisitor(IConsole console)
    {
        _console = console;
    }

    /// <inheritdoc/>
    public void Visit(Directory directory)
    {
        _console.WriteLine($"Visiting: {directory.GetPath()}");
        foreach (var child in directory.Children)
            child.Accept(this);
    }

    /// <inheritdoc/>
    public void Visit(WordDocument file)
    {
        _console.WriteLine($"Visiting: {file.GetPath()}");
        TotalSize += file.Size;
    }

    /// <inheritdoc/>
    public void Visit(ImageFile file)
    {
        _console.WriteLine($"Visiting: {file.GetPath()}");
        TotalSize += file.Size;
    }

    /// <inheritdoc/>
    public void Visit(TextFile file)
    {
        _console.WriteLine($"Visiting: {file.GetPath()}");
        TotalSize += file.Size;
    }
}
