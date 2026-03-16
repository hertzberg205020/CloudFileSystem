namespace CloudFileSystem.ConsoleApp.Commands;

/// <summary>
/// 封裝可執行且可復原的操作，為 Command Pattern 中的 Command 角色。
/// </summary>
public interface ICommand
{
    /// <summary>
    /// 執行此操作。
    /// </summary>
    void Execute();

    /// <summary>
    /// 復原此操作，將狀態還原至 <see cref="Execute"/> 執行前。
    /// </summary>
    void Undo();

    /// <summary>
    /// 取得此操作的描述文字，用於 undo/redo 提示。
    /// </summary>
    string Description { get; }
}
