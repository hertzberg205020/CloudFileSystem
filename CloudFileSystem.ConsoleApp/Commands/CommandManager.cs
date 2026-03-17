namespace CloudFileSystem.ConsoleApp.Commands;

/// <summary>
/// 管理 Command 歷史的 Invoker，透過兩個 Stack 支援 undo/redo。
/// </summary>
public class CommandManager
{
    private readonly Stack<ICommand> _undoStack = new();
    private readonly Stack<ICommand> _redoStack = new();

    /// <summary>
    /// 取得是否有可復原的操作。
    /// </summary>
    public bool CanUndo => _undoStack.Count > 0;

    /// <summary>
    /// 取得是否有可重做的操作。
    /// </summary>
    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>
    /// 執行指定的 Command 並記錄至 undo 歷史，同時清空 redo 歷史。
    /// </summary>
    /// <param name="command">要執行的 Command。</param>
    public void Execute(ICommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear();
    }

    /// <summary>
    /// 復原最近一次執行的操作，並將其移至 redo 歷史。
    /// </summary>
    /// <returns>被復原的 Command；若無可復原的操作則回傳 <see langword="null"/>。</returns>
    public ICommand? Undo()
    {
        if (!CanUndo)
            return null;

        var command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);
        return command;
    }

    /// <summary>
    /// 重做最近一次復原的操作，並將其移回 undo 歷史。
    /// </summary>
    /// <returns>被重做的 Command；若無可重做的操作則回傳 <see langword="null"/>。</returns>
    public ICommand? Redo()
    {
        if (!CanRedo)
            return null;

        var command = _redoStack.Pop();
        command.Execute();
        _undoStack.Push(command);
        return command;
    }
}
