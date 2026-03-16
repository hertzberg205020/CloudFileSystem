using CloudFileSystem.ConsoleApp.Models;

namespace CloudFileSystem.ConsoleApp.Visitors;

/// <summary>
/// 依副檔名搜尋檔案的 Visitor，對應功能二-2。
/// </summary>
/// <remarks>
/// <para>搜尋結果（檔案完整路徑）可透過 <see cref="Results"/> 屬性取得。</para>
/// <para>走訪過程中會透過 <see cref="IConsole"/> 輸出 Traverse Log。</para>
/// </remarks>
public class SearchByExtensionVisitor : IFileSystemVisitor
{
    private readonly string _targetExtension;
    private readonly IConsole _console;

    /// <summary>
    /// 取得搜尋結果中所有符合的檔案路徑。
    /// </summary>
    /// <value>符合副檔名的檔案完整路徑集合。若未找到任何符合的檔案，為空集合。</value>
    public List<string> Results { get; } = [];

    /// <summary>
    /// 初始化 <see cref="SearchByExtensionVisitor"/> 的新執行個體。
    /// </summary>
    /// <param name="extension">要搜尋的副檔名，包含或不包含前導點號皆可（例如 <c>".docx"</c> 或 <c>"docx"</c>）。</param>
    /// <param name="console">用於輸出 Traverse Log 的 Console 抽象。</param>
    public SearchByExtensionVisitor(string extension, IConsole console)
    {
        _targetExtension = extension.StartsWith('.') ? extension : "." + extension;
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
    public void Visit(WordDocument file) => VisitFile(file);

    /// <inheritdoc/>
    public void Visit(ImageFile file) => VisitFile(file);

    /// <inheritdoc/>
    public void Visit(TextFile file) => VisitFile(file);

    private void VisitFile(FileSystemComponent file)
    {
        _console.WriteLine($"Visiting: {file.GetPath()}");
        if (file.Name.EndsWith(_targetExtension, StringComparison.OrdinalIgnoreCase))
            Results.Add(file.GetPath());
    }
}
