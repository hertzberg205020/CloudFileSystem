# CloudFileSystem 雲端檔案管理系統

一個以 Console 應用程式實作的雲端檔案管理系統，展示 **Composite + Visitor + Command + Prototype** 四個設計模式的應用。

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

啟動後會進入互動式命令列，以 `根目錄 (Root)>` 提示等待輸入。

### 檢視指令

| 指令 | 說明 | 範例 |
|------|------|------|
| `display` | 印出完整樹狀目錄結構（含標籤） | `display` |
| `size` | 遞迴計算根目錄總容量（含 Traverse Log） | `size` |
| `search <副檔名>` | 搜尋符合副檔名的檔案路徑 | `search .docx` |
| `xml` | 以 XML 格式輸出目錄結構 | `xml` |

### 編輯指令

| 指令 | 說明 | 範例 |
|------|------|------|
| `delete <名稱>` | 刪除指定的檔案或目錄 | `delete README.txt` |
| `copy <名稱>` | 複製指定元件至剪貼簿 | `copy 專案文件 (Project_Docs)` |
| `paste` | 將剪貼簿內容貼入根目錄（深拷貝，同名自動改名） | `paste` |

### 排序指令

| 指令 | 說明 | 範例 |
|------|------|------|
| `sort <欄位> <方向>` | 排序根目錄下的子元件 | `sort name asc` |

排序欄位：`name`、`size`、`ext`（副檔名）。排序方向：`asc`（升冪）、`desc`（降冪）。

### 標籤指令

| 指令 | 說明 | 範例 |
|------|------|------|
| `tag <名稱> <標籤>` | 為指定元件加上標籤 | `tag README.txt Urgent` |
| `untag <名稱> <標籤>` | 移除指定元件的標籤 | `untag README.txt Urgent` |

可用標籤：`Urgent`、`Work`、`Personal`。每個元件可同時擁有多個標籤。

### 狀態管理

| 指令 | 說明 |
|------|------|
| `undo` | 復原上一次的編輯操作 |
| `redo` | 重做上一次被復原的操作 |
| `exit` | 結束程式 |

所有編輯操作（delete、paste、sort、tag、untag）皆支援 undo/redo。

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

根目錄 (Root)> tag README.txt Urgent
Tagged README.txt as Urgent

根目錄 (Root)> tag README.txt Work
Tagged README.txt as Work

根目錄 (Root)> display
根目錄 (Root)
├── ...
└── README.txt [純文字檔] (編碼: ASCII, 大小: 500B) {Urgent, Work}

根目錄 (Root)> delete README.txt
Deleted: README.txt

根目錄 (Root)> undo
Undone: Delete README.txt

根目錄 (Root)> copy README.txt
Copied: README.txt

根目錄 (Root)> paste
Pasted: README.txt

根目錄 (Root)> sort name asc
Sorted by Name Asc

根目錄 (Root)> exit
```

## 架構概覽

```
CloudFileSystem.ConsoleApp/
├── Models/       ← Composite Pattern：FileSystemComponent, Directory, File 子類別 + Enums
├── Visitors/     ← Visitor Pattern：IFileSystemVisitor + 4 個具體 Visitor
├── Commands/     ← Command Pattern：ICommand + CommandManager + 5 個具體 Command
├── IConsole.cs   ← I/O 抽象介面（方便測試時注入 stub）
└── Program.cs    ← 進入點
```

- **Composite Pattern**：`Directory`（Composite）與 `File`（Leaf）共享 `FileSystemComponent` 抽象基底，支援無限層級嵌套。
- **Visitor Pattern**：將遍歷操作（顯示、計算大小、搜尋、XML 輸出）抽離為獨立 Visitor 類別，新增操作不需修改結構。
- **Command Pattern**：將突變操作封裝為 `ICommand`（`Execute()` + `Undo()`），透過 `CommandManager` 以兩個 Stack 管理 undo/redo 歷史。
- **Prototype Pattern**：`FileSystemComponent.DeepCopy()` 支援 Composite 樹狀結構的遞迴深拷貝，用於複製/貼上功能。
- **IConsole 抽象**：將標準 I/O 抽象為介面，測試時透過 stub 注入預設輸入並擷取輸出。
