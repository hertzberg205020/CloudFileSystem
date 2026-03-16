# CloudFileSystem 雲端檔案管理系統

一個以 Console 應用程式實作的雲端檔案管理系統，展示 **Composite Pattern** 與 **Visitor Pattern** 的設計模式應用。

## 環境需求

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## 建置與執行

```bash
# 建置
dotnet build

# 執行
dotnet run --project CloudFileSystem.ConsoleApp

# 執行測試
dotnet test
```

## CLI 指令

啟動後會進入互動式命令列，以 `根目錄 (Root)>` 提示等待輸入：

| 指令 | 說明 | 範例 |
|------|------|------|
| `display` | 印出完整樹狀目錄結構 | `display` |
| `size` | 遞迴計算根目錄總容量（含 Traverse Log） | `size` |
| `search <副檔名>` | 搜尋符合副檔名的檔案路徑（含 Traverse Log） | `search .docx` |
| `xml` | 以 XML 格式輸出目錄結構 | `xml` |
| `exit` | 結束程式 | `exit` |

### 範例操作

```
根目錄 (Root)> display
根目錄 (Root)
├── 專案文件 (Project_Docs) [目錄]
│   ├── 需求規格書.docx [Word 檔案] (頁數: 15, 大小: 500KB)
│   └── 系統架構圖.png [圖片] (解析度: 1920x1080, 大小: 2MB)
├── 個人筆記 (Personal_Notes) [目錄]
│   ├── 待辦清單.txt [純文字檔] (編碼: UTF-8, 大小: 1KB)
│   └── 2025備份 (Archive_2025) [子目錄]
│       └── 舊會議記錄.docx [Word 檔案] (頁數: 5, 大小: 200KB)
└── README.txt [純文字檔] (編碼: ASCII, 大小: 500B)

根目錄 (Root)> search .docx
Found 2 file(s):
  根目錄 (Root)/專案文件 (Project_Docs)/需求規格書.docx
  根目錄 (Root)/個人筆記 (Personal_Notes)/2025備份 (Archive_2025)/舊會議記錄.docx

根目錄 (Root)> exit
```

## 架構概覽

```
CloudFileSystem.ConsoleApp/
├── Models/       ← Composite Pattern：FileSystemComponent, Directory, File 子類別
├── Visitors/     ← Visitor Pattern：IFileSystemVisitor + 4 個具體 Visitor
├── IConsole.cs   ← I/O 抽象介面（方便測試時注入 stub）
└── Program.cs    ← 進入點
```

- **Composite Pattern**：`Directory`（Composite）與 `File`（Leaf）共享 `FileSystemComponent` 抽象基底，支援無限層級嵌套。
- **Visitor Pattern**：將遍歷操作（顯示、計算大小、搜尋、XML 輸出）抽離為獨立 Visitor 類別，新增操作不需修改結構。
- **IConsole 抽象**：將標準 I/O 抽象為介面，測試時透過 stub 注入預設輸入並擷取輸出，無需真正的 stdin/stdout。
