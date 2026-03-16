namespace CloudFileSystem.ConsoleApp;

/// <summary>
/// 抽象化 Console I/O 操作的介面，使 CLI 可透過依賴注入進行測試。
/// </summary>
/// <remarks>
/// 生產環境使用 <see cref="SystemConsole"/>；測試時注入 stub 以模擬輸入並擷取輸出。
/// </remarks>
public interface IConsole
{
    /// <summary>
    /// 從輸入來源讀取一行文字。
    /// </summary>
    /// <returns>讀取到的文字；輸入結束時回傳 <see langword="null"/>。</returns>
    string? ReadLine();

    /// <summary>
    /// 將文字寫入標準輸出，不換行。
    /// </summary>
    /// <param name="text">要輸出的文字。</param>
    void Write(string text);

    /// <summary>
    /// 將文字寫入標準輸出，並換行。
    /// </summary>
    /// <param name="text">要輸出的文字。</param>
    void WriteLine(string text);

    /// <summary>
    /// 將錯誤訊息寫入標準錯誤輸出。
    /// </summary>
    /// <param name="text">錯誤訊息內容。</param>
    void WriteError(string text);
}
