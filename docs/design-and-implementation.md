# 設計與實作概念

## 一、專案概覽

### 系統目標與範圍

本專案實作一個**雲端檔案管理系統**的核心領域模型與互動式 CLI，涵蓋：

- 檔案與目錄的遞迴樹狀結構建模
- 四種唯讀操作：樹狀結構顯示、容量計算、副檔名搜尋、XML 匯出
- 五種狀態改變操作：刪除、複製/貼上、排序、標籤/取消標籤
- 所有突變操作的 undo/redo 支援

重點在於物件導向分析與設計，而非圖形介面，因此以 Console 文字輸出呈現結果。

### 專案結構總覽

```txt
CloudFileSystem/
├── CloudFileSystem.ConsoleApp/
│   ├── Models/              ← Composite 層：FileSystemComponent, Directory, File, WordDocument, ImageFile, TextFile, Tag, SortBy, SortOrder
│   ├── Visitors/            ← Visitor 層：IFileSystemVisitor + 4 個 ConcreteVisitor
│   ├── Commands/            ← Command 層：ICommand + CommandManager + 5 個 ConcreteCommand
│   ├── CloudFileSystemCli.cs  ← CLI controller，持有 CommandManager 與 _clipboard
│   ├── IConsole.cs          ← I/O 抽象介面
│   └── SystemConsole.cs     ← 生產環境的 Console 實作
├── CloudFileSystem.Tests/   ← xUnit 測試專案（TestConsole 注入假輸入/擷取輸出）
├── test-cases/              ← Golden files（plain-text.out, xml-format.out）
├── spec/class-diagram.md    ← Mermaid UML 類別圖
└── docs/                    ← 需求規格、ER Model、OOA/OOD 圖、本文件
```

每個目錄對應一個設計模式的職責邊界：`Models/` 承載 Composite + Prototype，`Visitors/` 承載 Visitor，`Commands/` 承載 Command。CLI 作為 Client 串聯三者。

---

## 二、物件導向分析（OOA）— 從需求訪談到領域模型

### 2.1 需求萃取

從客戶訪談紀錄中，以**名詞 → 實體、動詞 → 操作**的原則進行拆解：

| 訪談內容                                                | 名詞（實體候選）            | 動詞（操作候選） |
| ------------------------------------------------------- | --------------------------- | ---------------- |
| 「我們有 Word 文件、圖片、純文字檔」                    | Word 文件、圖片、純文字檔   | —                |
| 「每個檔案都要有檔名、大小、建立時間」                  | 檔名、大小、建立時間        | —                |
| 「Word 要記錄頁數；圖片要記錄解析度；純文字要記錄編碼」 | 頁數、解析度（寬/高）、編碼 | —                |
| 「用目錄來管理，目錄可以一層套一層」                    | 目錄                        | 管理、嵌套       |
| 「所有檔案必須放在某個目錄下面，不能孤零零地存在外面」  | —                           | 放入（複合關係） |

**識別出的實體**：

- **檔案**（三種具體類型）：Word 文件、圖片、純文字檔
- **目錄**：可包含檔案與子目錄
- **標籤**：Urgent、Work、Personal（來自進階功能需求）

**識別出的關係**：

- 目錄 → 檔案：**複合（Composition）**（檔案不能脫離目錄獨立存在，生命週期依附於所屬目錄）
- 目錄 → 子目錄：**遞迴複合**（目錄可無限嵌套，子目錄同樣依附於父目錄）
- 元件 → 標籤：**多對多**（任何元件可有多個標籤，同一標籤可標記多個元件）

### 2.2 領域模型推導

#### 共同屬性抽象

三種檔案皆有**檔名、大小、建立時間**，而目錄也有**名稱**。這指向一個**抽象基底類別**，統一所有元件的共同介面：

```txt
FileSystemComponent（抽象）
├── Name：所有元件共有
├── GetSize()：目錄遞迴加總，檔案回傳自身大小
└── Parent：維護父子關係，根目錄為 null
```

#### 差異屬性識別

各檔案類型有獨特屬性，自然形成子類別特化：

| 類型      | 專屬屬性                |
| --------- | ----------------------- |
| Word 文件 | PageCount（頁數）       |
| 圖片      | Width, Height（解析度） |
| 純文字檔  | Encoding（編碼）        |

#### 統一介面原則

客戶訪談中的「目錄可以一層套一層」暗示了一個關鍵需求：**外部操作不需要區分目錄與檔案**。無論是計算大小、刪除、複製，都應該以相同方式操作。這是後續選用 Composite Pattern 的核心驅動力。

#### 領域模型結構

```txt
FileSystemComponent（抽象父類別）
├── Directory（持有 children 清單）
└── File（抽象，持有 Size + CreatedAt）
    ├── WordDocument（+ PageCount）
    ├── ImageFile（+ Width, Height）
    └── TextFile（+ Encoding）
```

> 參照 OOA 圖：[`docs/OOA/ooa.jpg`](OOA/ooa.jpg)

### 2.3 功能需求與操作分類

將功能需求依**是否修改資料結構**分為兩類，此分類直接驅動後續設計模式的選擇：

#### 唯讀操作（不改變結構，不需 undo）

| 操作         | 說明                             | 驅動的設計需求                           |
| ------------ | -------------------------------- | ---------------------------------------- |
| 顯示樹狀結構 | 以 Box-drawing 字元渲染完整樹    | 需遍歷整棵樹，且格式化邏輯不應寫在模型中 |
| 計算容量     | 遞迴加總指定目錄下所有檔案大小   | 需遍歷整棵子樹 + 輸出 Traverse Log       |
| 搜尋         | 依副檔名篩選，列出符合的檔案路徑 | 需遍歷整棵樹 + 輸出 Traverse Log         |
| XML 輸出     | 將結構序列化為 XML 格式          | 需遍歷整棵樹，且序列化邏輯不應寫在模型中 |

**共同特徵**：都需要遍歷樹、都是在結構上執行不同的「操作」、都不應讓模型類別承擔格式化責任。
→ 這驅動了 **Visitor Pattern** 的選用。

#### 狀態修改操作（修改結構，需 undo/redo）

| 操作          | 說明                   | 驅動的設計需求                    |
| ------------- | ---------------------- | --------------------------------- |
| 刪除          | 從父目錄移除元件       | 需記錄原始位置以支援 undo 還原    |
| 複製/貼上     | 複製元件到目標目錄     | 需深拷貝整棵子樹，避免共享參照    |
| 排序          | 依名稱/大小/副檔名排序 | 需記錄排序前的完整順序以支援 undo |
| 標籤/取消標籤 | 為元件加上或移除標籤   | 操作本身即為互逆（tag ↔ untag）   |

**共同特徵**：都是「改變狀態」的操作、都需要可復原（undo/redo）、需要有序的歷史管理。
→ 這驅動了 **Command Pattern** 的選用。

#### 特殊需求：深拷貝

複製/貼上需要產生**完全獨立的副本**。由於目錄可無限嵌套，拷貝必須遞迴處理整棵子樹，且每個元件自行負責自己的克隆邏輯。
→ 這驅動了 **Prototype Pattern** 的選用。

**小結**：OOA 階段識別的三個核心問題——「統一操作遞迴結構」、「解耦遍歷操作與資料結構」、「可復原的突變操作」——分別對應了 Composite、Visitor、Command 三個設計模式的選用，再加上深拷貝需求引入的 Prototype，構成了本專案的四個設計模式。
