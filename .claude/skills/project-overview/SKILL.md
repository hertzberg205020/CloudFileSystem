---
name: project-overview
description: "CloudFileSystem 雲端檔案管理系統的專案總覽。包含需求摘要、設計模式（Composite + Visitor + Command + Prototype Pattern）、C# 類別結構、實作計畫與範例資料。同時涵蓋測試專案結構（CloudFileSystem.Tests）、IConsole I/O 抽象、CLI 互動式指令迴圈模式、以及 golden file 測試策略。當你需要了解這個專案在做什麼、為什麼這樣設計、類別之間的關係、或是要開始寫程式碼之前，都應該先讀這份 skill。任何涉及新增類別、修改結構、撰寫 Visitor、撰寫 Command、調整 CLI、討論架構決策、或撰寫測試時，務必參考此 skill。"
---

# CloudFileSystem 雲端檔案管理系統 — 專案總覽

## 專案目的

實作一個雲端檔案管理系統的 Console 應用程式，展示物件導向分析能力與設計模式的應用。重點在於架構思維與核心邏輯，而非 GUI。

## 技術棧

- **語言**：C# (.NET 10)
- **專案結構**：`CloudFileSystem.sln` → `CloudFileSystem.ConsoleApp` + `CloudFileSystem.Tests`（xUnit）
- **輸出**：Console 應用程式

## 需求摘要

完整需求文件位於 `docs/requirements.md`。

### 領域模型

| 概念 | 說明 |
|------|------|
| **檔案類型** | Word（頁數）、圖片（解析度寬高）、純文字（編碼） |
| **共同屬性** | 檔名、大小 (KB)、建立時間 |
| **組織方式** | 目錄可包含檔案與子目錄，無限層級嵌套 |
| **約束** | 所有檔案必須在某個目錄下，目錄也有名稱 |

### 功能需求（功能一～三）

1. **目錄結構呈現**（功能一）：初始化範例結構，印出完整樹狀結構與檔案詳細資訊
2. **核心邏輯**（功能二）：
   - 遞迴計算任一目錄的總容量
   - 副檔名搜尋（回傳檔案路徑）
   - XML 結構輸出
3. **進度追蹤**（功能三）：執行計算/搜尋時印出走訪節點順序 (Traverse Log)

### 進階功能（功能四 Bonus）

1. **排序功能**：按名稱、大小、副檔名排序（持久修改 `_children` 順序，可 undo）
2. **編輯功能**：刪除節點、複製/貼上（深拷貝子樹，同名自動改名加後綴）
3. **標籤功能**：適用於所有 `FileSystemComponent`，預定義 `Tag` enum（Urgent、Work、Personal），支援多重標籤
4. **狀態管理 Undo/Redo**：所有突變操作（delete、paste、sort、tag、untag）經過 `CommandManager`

### 範例結構

```
根目錄 (Root)
├── 專案文件 (Project_Docs) [目錄]
│   ├── 需求規格書.docx [Word] (頁數: 15, 大小: 500KB)
│   └── 系統架構圖.png [圖片] (解析度: 1920x1080, 大小: 2MB)
├── 個人筆記 (Personal_Notes) [目錄]
│   ├── 待辦清單.txt [純文字] (編碼: UTF-8, 大小: 1KB)
│   └── 2025備份 (Archive_2025) [子目錄]
│       └── 舊會議記錄.docx [Word] (頁數: 5, 大小: 200KB)
└── README.txt [純文字] (編碼: ASCII, 大小: 500B)
```

## 設計模式

本專案套用四個設計模式：

### Composite Pattern — 解決結構變動性

目錄與檔案形成樹狀結構。引入 `FileSystemComponent` 抽象類別作為 Directory（Composite）和 File（Leaf）的共同父類別，讓 Directory 持有 `List<FileSystemComponent>` 而非分別的 Directory/File 集合。

**好處**：新增檔案類型時，只需繼承 `File`，不需修改 Directory 或 CLI。

### Visitor Pattern — 解決操作變動性

多種走訪操作（顯示、計算大小、搜尋、XML 輸出）作用於同一結構。將操作邏輯從結構中抽離到獨立的 Visitor 類別。

**好處**：新增操作時，只需實作新的 `IFileSystemVisitor`，不需修改任何 Component 類別。

### Command Pattern — 解決操作可逆性

突變操作（delete、paste、sort、tag、untag）需要支援 undo/redo。將每個操作封裝為實作 `ICommand`（`Execute()` + `Undo()`）的獨立類別，透過 `CommandManager` 以兩個 `Stack<ICommand>` 管理歷史。

**好處**：新增突變操作時，只需實作新的 `ICommand`，不需修改 CommandManager 或 CLI 的 undo/redo 機制。

### Prototype Pattern — 解決 Composite 深拷貝

複製/貼上功能需要對可能包含多層巢狀的 Composite 子樹產生完全獨立的副本。在 `FileSystemComponent` 定義 `abstract DeepCopy()`，每個 concrete class 實作自己的複製邏輯。`Directory.DeepCopy()` 遞迴呼叫 `child.DeepCopy()` 並透過 `Add()` 維護 `Parent` 參照。

**好處**：新增檔案類型時，只需在新類別實作 `DeepCopy()`，不需修改 Directory 或 PasteCommand。

### OCP 分析

- **Open for extension**：新檔案類型（加 File 子類別 + DeepCopy）、新操作（加 Visitor）、新突變操作（加 Command）
- **Closed for modification**：Directory、CLI、CommandManager、現有 Visitor 與 Command 不需改動

## 類別結構（C# 命名慣例）

完整 Mermaid 類別圖位於 `spec/class-diagram.md`。以下是結構摘要：

### Composite 層

| 類別 | 角色 | 關鍵成員 |
|------|------|----------|
| `FileSystemComponent` | Component (abstract) | `Name`, `Parent`, `Tags`, `GetSize()`, `Accept()`, `DeepCopy()`, `GetPath()`, `AddTag()`, `RemoveTag()` |
| `Directory` | Composite | `Children`, `Add()`, `Remove()`, `Insert()`, `IndexOf()`, `Sort()`, `SetChildrenOrder()`, `DeepCopy()` |
| `File` | Leaf base (abstract) | `Size`, `CreatedAt`, `FormatSize()` |
| `WordDocument` | Leaf | `PageCount`, `DeepCopy()` |
| `ImageFile` | Leaf | `Width`, `Height`, `DeepCopy()` |
| `TextFile` | Leaf | `Encoding`, `DeepCopy()` |

### Enum

| Enum | 值 |
|------|-----|
| `Tag` | `Urgent`, `Work`, `Personal` |
| `SortBy` | `Name`, `Size`, `Extension` |
| `SortOrder` | `Asc`, `Desc` |

### Visitor 層

| 類別 | 對應功能 |
|------|----------|
| `IFileSystemVisitor` | 介面：`Visit(Directory)`, `Visit(WordDocument)`, `Visit(ImageFile)`, `Visit(TextFile)` |
| `DisplayVisitor` | 功能一：印出樹狀結構（含標籤顯示） |
| `SizeCalculatorVisitor` | 功能二-1：遞迴計算總容量 |
| `SearchByExtensionVisitor` | 功能二-2：副檔名搜尋 |
| `XmlExportVisitor` | 功能二-3：XML 結構輸出 |

Traverse Log（功能三）整合在各 Visitor 的 Visit 方法中。

### Command 層

| 類別 | 對應功能 |
|------|----------|
| `ICommand` | 介面：`Execute()`, `Undo()`, `Description` |
| `CommandManager` | Invoker：兩個 `Stack<ICommand>`（undoStack / redoStack） |
| `DeleteCommand` | 刪除節點，Undo = 插回原位置 |
| `PasteCommand` | 深拷貝 clipboard 並加入目錄，Undo = 移除 |
| `SortCommand` | 排序 children，Undo = 還原原始順序 |
| `TagCommand` | 加標籤，Undo = 移除標籤 |
| `UntagCommand` | 移除標籤，Undo = 加回標籤 |

### I/O 抽象層

| 類別 | 角色 |
|------|------|
| `IConsole` | 介面：`ReadLine()`, `Write(string)`, `WriteLine(string)`, `WriteError(string)` |
| `SystemConsole` | 生產實作：委派至 `System.Console` 對應方法 |

### CLI 層

`CloudFileSystemCli` 是互動式 CLI controller，透過建構子注入 `IConsole` 與根目錄 `Directory`，持有 `CommandManager` 和 `_clipboard`。

#### 互動式指令迴圈

1. 顯示提示 `{current.Name}> `（透過 `IConsole.Write`）
2. 讀取使用者輸入（透過 `IConsole.ReadLine`）
3. 解析指令 → 唯讀指令直接執行 / 突變指令透過 CommandManager 執行
4. 迴圈直到使用者輸入 `exit` 或 `ReadLine` 回傳 `null`

#### CLI 指令對照表

| 指令 | 分類 | 處理方式 |
|------|------|---------|
| `display` | 唯讀 | `DisplayVisitor` → `GetOutput()` |
| `size` | 唯讀 | `SizeCalculatorVisitor` → `TotalSize` |
| `search <ext>` | 唯讀 | `SearchByExtensionVisitor` → `Results` |
| `xml` | 唯讀 | `XmlExportVisitor` → `GetXml()` |
| `copy <name>` | 唯讀 | 存入 `_clipboard`（不走 Command） |
| `delete <name>` | 突變 | `CommandManager.Execute(DeleteCommand)` |
| `paste` | 突變 | `CommandManager.Execute(PasteCommand)` |
| `sort <by> <order>` | 突變 | `CommandManager.Execute(SortCommand)` |
| `tag <name> <tag>` | 突變 | `CommandManager.Execute(TagCommand)` |
| `untag <name> <tag>` | 突變 | `CommandManager.Execute(UntagCommand)` |
| `undo` | 管理 | `CommandManager.Undo()` |
| `redo` | 管理 | `CommandManager.Redo()` |
| `exit` | 終止 | 結束迴圈 |

## 專案目錄結構規劃

```
CloudFileSystem/
├── CloudFileSystem.sln
├── CloudFileSystem.ConsoleApp/
│   ├── Models/          ← Composite 層 + Enums
│   ├── Visitors/        ← Visitor 層：IFileSystemVisitor + 4 個具體 Visitor
│   ├── Commands/        ← Command 層：ICommand + CommandManager + 5 個具體 Command
│   ├── IConsole.cs      ← I/O 抽象介面
│   ├── SystemConsole.cs ← IConsole 生產實作
│   ├── CloudFileSystemCli.cs  ← CLI controller（注入 IConsole + Directory）
│   └── Program.cs       ← 進入點
├── CloudFileSystem.Tests/
│   ├── Models/          ← Composite 層測試
│   ├── Visitors/        ← Visitor golden file 比對測試
│   ├── Commands/        ← Command + Undo/Redo 測試
│   └── Cli/             ← CLI 整合測試（使用 IConsole stub）
├── test-cases/          ← Golden file 測試資料
│   ├── plain-text.out   ← DisplayVisitor 預期輸出
│   └── xml-format.out   ← XmlExportVisitor 預期輸出
├── docs/
│   ├── requirements.md
│   └── OOA/
└── spec/
    └── class-diagram.md ← Mermaid UML 類別圖（含四個設計模式）
```

## 測試策略

測試方法論與 xUnit 用法細節請參考 `csharp-tdd` 與 `xunit-best-practices` skill。以下僅描述本專案特定的測試架構。

### 測試專案

- **專案名稱**：`CloudFileSystem.Tests`（xUnit）
- **專案引用**：`CloudFileSystem.ConsoleApp`
- **執行指令**：`dotnet test`

### IConsole Stub 測試模式

測試 `CloudFileSystemCli` 時，實作一個 stub `IConsole`：

- 建構時傳入預設的輸入佇列（模擬使用者輸入）
- 擷取所有 `Write`/`WriteLine` 呼叫的輸出至內部 buffer
- 測試結束後比對 buffer 內容與預期結果

### Golden File 比對

`test-cases/` 目錄包含預期輸出檔案（golden files）：

| 檔案 | 對應 Visitor | 用途 |
|------|-------------|------|
| `plain-text.out` | `DisplayVisitor` | 驗證樹狀結構文字輸出 |
| `xml-format.out` | `XmlExportVisitor` | 驗證 XML 結構輸出 |

### Command 測試重點

- 每個 Command 的 Execute + Undo 正確性
- CommandManager 的 undo/redo stack 狀態轉換
- 連續 undo/redo 的順序正確性
- DeepCopy 的遞迴複製與 Parent 參照完整性

## XML 輸出格式參考

```xml
<根目錄_Root>
    <專案文件_Project_Docs>
        <需求規格書_docx>頁數: 15, 大小: 500KB</需求規格書_docx>
        <系統架構圖_png>解析度: 1920x1080, 大小: 2MB</系統架構圖_png>
    </專案文件_Project_Docs>
    <個人筆記_Personal_Notes>
        <待辦清單_txt>編碼: UTF-8, 大小: 1KB</待辦清單_txt>
        <Archive_2025>
            <舊會議記錄_docx>頁數: 5, 大小: 200KB</舊會議記錄_docx>
        </Archive_2025>
    </個人筆記_Personal_Notes>
    <README_txt>編碼: ASCII, 大小: 500B</README_txt>
</根目錄_Root>
```
