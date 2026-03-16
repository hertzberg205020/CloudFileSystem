---
name: project-overview
description: "CloudFileSystem 雲端檔案管理系統的專案總覽。包含需求摘要、設計模式（Composite + Visitor Pattern）、C# 類別結構、實作計畫與範例資料。當你需要了解這個專案在做什麼、為什麼這樣設計、類別之間的關係、或是要開始寫程式碼之前，都應該先讀這份 skill。任何涉及新增類別、修改結構、撰寫 Visitor、調整 CLI、或討論架構決策時，務必參考此 skill。"
---

# CloudFileSystem 雲端檔案管理系統 — 專案總覽

## 專案目的

實作一個雲端檔案管理系統的 Console 應用程式，展示物件導向分析能力與設計模式的應用。重點在於架構思維與核心邏輯，而非 GUI。

## 技術棧

- **語言**：C# (.NET 10)
- **IDE**：JetBrains Rider
- **專案結構**：`CloudFileSystem.sln` → `CloudFileSystem.ConsoleApp`
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

### CLI 層

`CloudFileSystemCli` 是薄 controller：接收使用者指令 → 建立對應 Visitor → 呼叫 `Root.Accept(visitor)` → 輸出結果。

## 專案目錄結構規劃

```
CloudFileSystem/
├── CloudFileSystem.sln
├── CloudFileSystem.ConsoleApp/
│   ├── Models/          ← Composite 層：FileSystemComponent, Directory, File, 子類別
│   ├── Visitors/        ← Visitor 層：IFileSystemVisitor + 4 個具體 Visitor
│   ├── CloudFileSystemCli.cs  ← CLI controller
│   └── Program.cs       ← 進入點：初始化範例結構、啟動 CLI
├── docs/
│   ├── requirements.md  ← 完整需求文件
│   └── OOA/             ← 物件導向分析圖
└── spec/
    └── class-diagram.md ← 套用設計模式後的 Mermaid UML 類別圖
```

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
