---
name: project-overview
description: "CloudFileSystem 雲端檔案管理系統的專案總覽。包含需求摘要、設計模式（Composite + Visitor Pattern）、C# 類別結構、實作計畫與範例資料。同時涵蓋測試專案結構（CloudFileSystem.Tests）、IConsole I/O 抽象、CLI 互動式指令迴圈模式、以及 golden file 測試策略。當你需要了解這個專案在做什麼、為什麼這樣設計、類別之間的關係、或是要開始寫程式碼之前，都應該先讀這份 skill。任何涉及新增類別、修改結構、撰寫 Visitor、調整 CLI、討論架構決策、或撰寫測試時，務必參考此 skill。"
---

# CloudFileSystem 雲端檔案管理系統 — 專案總覽

## 專案目的

實作一個雲端檔案管理系統的 Console 應用程式，展示物件導向分析能力與設計模式的應用。重點在於架構思維與核心邏輯，而非 GUI。

## 技術棧

- **語言**：C# (.NET 10)
- **專案結構**：`CloudFileSystem.sln` → `CloudFileSystem.ConsoleApp` + `CloudFileSystem.Tests`（xUnit）
- **輸出**：Console 應用程式

## 需求摘要（排除 Bonus 功能四）

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

本專案套用兩個設計模式解決擴充性問題：

### Composite Pattern — 解決結構變動性

目錄與檔案形成樹狀結構。引入 `FileSystemComponent` 抽象類別作為 Directory（Composite）和 File（Leaf）的共同父類別，讓 Directory 持有 `List<FileSystemComponent>` 而非分別的 Directory/File 集合。

**好處**：新增檔案類型時，只需繼承 `File`，不需修改 Directory 或 CLI。

### Visitor Pattern — 解決操作變動性

多種遍歷操作（顯示、計算大小、搜尋、XML 輸出）作用於同一結構。將操作邏輯從結構中抽離到獨立的 Visitor 類別。

**好處**：新增操作時，只需實作新的 `IFileSystemVisitor`，不需修改任何 Component 類別。

### OCP 分析

- **Open for extension**：新檔案類型（加 File 子類別）、新操作（加 Visitor）
- **Closed for modification**：Directory、CLI、現有 Visitor 不需改動

## 類別結構（C# 命名慣例）

完整 Mermaid 類別圖位於 `spec/class-diagram.md`。以下是結構摘要：

### Composite 層

| 類別 | 角色 | 關鍵成員 |
|------|------|----------|
| `FileSystemComponent` | Component (abstract) | `Name`, `Parent`, `GetSize()`, `Accept(IFileSystemVisitor)`, `GetPath()` |
| `Directory` | Composite | `Children`, `Add()`, `Remove()` — GetSize() 遞迴加總 |
| `File` | Leaf base (abstract) | `Size`, `CreatedAt` — GetSize() 回傳 Size |
| `WordDocument` | Leaf | `PageCount` |
| `ImageFile` | Leaf | `Width`, `Height` |
| `TextFile` | Leaf | `Encoding` |

### Visitor 層

| 類別 | 對應功能 |
|------|----------|
| `IFileSystemVisitor` | 介面：`Visit(Directory)`, `Visit(WordDocument)`, `Visit(ImageFile)`, `Visit(TextFile)` |
| `DisplayVisitor` | 功能一：印出樹狀結構 |
| `SizeCalculatorVisitor` | 功能二-1：遞迴計算總容量 |
| `SearchByExtensionVisitor` | 功能二-2：副檔名搜尋 |
| `XmlExportVisitor` | 功能二-3：XML 結構輸出 |

Traverse Log（功能三）整合在各 Visitor 的 Visit 方法中。

### I/O 抽象層

將 `System.Console` 的 I/O 操作抽象為 `IConsole` 介面，使 CLI 在生產環境用 `SystemConsole`，測試時用 stub 注入預設輸入並擷取輸出。

| 類別 | 角色 |
|------|------|
| `IConsole` | 介面：`ReadLine()`, `Write(string)`, `WriteLine(string)`, `WriteError(string)` |
| `SystemConsole` | 生產實作：委派至 `System.Console` 對應方法 |

檔案位置：`CloudFileSystem.ConsoleApp/IConsole.cs`、`CloudFileSystem.ConsoleApp/SystemConsole.cs`

### CLI 層

`CloudFileSystemCli` 是互動式 CLI controller，透過建構子注入 `IConsole` 與根目錄 `Directory`。

#### 互動式指令迴圈

參考景點-複合模式 v2 CLI 互動模式，`CloudFileSystemCli` 採用指令迴圈：

1. 顯示提示 `{current.Name}> `（透過 `IConsole.Write`）
2. 讀取使用者輸入（透過 `IConsole.ReadLine`）
3. 解析指令 → 建立對應 Visitor → 呼叫 `Accept(visitor)` → 透過 `IConsole` 輸出結果
4. 迴圈直到使用者輸入結束指令或 `ReadLine` 回傳 `null`

支援的指令對應 Visitor：

| 指令 | 對應 Visitor |
|------|-------------|
| 顯示結構 | `DisplayVisitor` |
| 計算大小 | `SizeCalculatorVisitor` |
| 搜尋副檔名 | `SearchByExtensionVisitor` |
| XML 輸出 | `XmlExportVisitor` |

## 專案目錄結構規劃

```
CloudFileSystem/
├── CloudFileSystem.sln
├── CloudFileSystem.ConsoleApp/
│   ├── Models/          ← Composite 層：FileSystemComponent, Directory, File, 子類別
│   ├── Visitors/        ← Visitor 層：IFileSystemVisitor + 4 個具體 Visitor
│   ├── IConsole.cs      ← I/O 抽象介面
│   ├── SystemConsole.cs ← IConsole 生產實作（委派至 System.Console）
│   ├── CloudFileSystemCli.cs  ← CLI controller（注入 IConsole + Directory）
│   └── Program.cs       ← 進入點：初始化範例結構、建立 SystemConsole、啟動 CLI
├── CloudFileSystem.Tests/     ← xUnit 測試專案
│   ├── CloudFileSystem.Tests.csproj  ← 引用 CloudFileSystem.ConsoleApp
│   ├── Visitors/        ← Visitor 輸出的 golden file 比對測試
│   └── Cli/             ← CLI 指令迴圈的整合測試（使用 IConsole stub）
├── test-cases/          ← Golden file 測試資料（預期輸出）
│   ├── plain-text.out   ← DisplayVisitor 預期輸出
│   └── xml-format.out   ← XmlExportVisitor 預期輸出
├── docs/
│   ├── requirements.md  ← 完整需求文件
│   └── OOA/             ← 物件導向分析圖
└── spec/
    └── class-diagram.md ← 套用設計模式後的 Mermaid UML 類別圖
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

這讓 CLI 整合測試無需真正的 stdin/stdout，可完全在記憶體中執行。

### Golden File 比對

`test-cases/` 目錄包含預期輸出檔案（golden files）：

| 檔案 | 對應 Visitor | 用途 |
|------|-------------|------|
| `plain-text.out` | `DisplayVisitor` | 驗證樹狀結構文字輸出 |
| `xml-format.out` | `XmlExportVisitor` | 驗證 XML 結構輸出 |

測試流程：
1. 建立與 golden file 相同的範例結構
2. 執行對應 Visitor
3. 比對 Visitor 輸出與檔案內容（字串比較）

Golden file 路徑可透過相對路徑 `../../../test-cases/` 從測試專案存取，或在 csproj 中設定 `<Content>` 複製到輸出目錄。

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
