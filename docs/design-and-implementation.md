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

從客戶訪談紀錄中，以**名詞、動詞萃取**的原則逐句拆解。注意：名詞在此階段僅為候選，尚未區分是類別還是屬性。

| 訪談內容                                                | 名詞                        | 動詞（操作候選） |
| ------------------------------------------------------- | --------------------------- | ---------------- |
| 「我們有 Word 文件、圖片、純文字檔」                    | Word 文件、圖片、純文字檔   | —                |
| 「每個檔案都要有檔名、大小、建立時間」                  | 檔名、大小、建立時間        | —                |
| 「Word 要記錄頁數；圖片要記錄解析度；純文字要記錄編碼」 | 頁數、解析度（寬/高）、編碼 | —                |
| 「用目錄來管理，目錄可以一層套一層」                    | 目錄                        | 管理、嵌套       |
| 「所有檔案必須放在某個目錄下面，不能孤零零地存在外面」  | —                           | 放入（複合關係） |

#### 名詞分類

萃取出的名詞需進一步篩選——並非所有名詞都是類別，有些是類別的屬性：

| 分類 | 名詞 | 判斷依據 |
| ---- | ---- | -------- |
| **類別候選** | Word 文件、圖片、純文字檔、目錄 | 具有獨立行為與狀態，能作為操作對象 |
| **屬性候選** | 檔名、大小、建立時間、頁數、解析度（寬/高）、編碼 | 描述某個類別的特徵，不能獨立存在 |

類別候選在後續 2.2 節推導為領域模型中的類別；屬性候選則歸入對應類別成為其欄位。

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

---

## 三、物件導向設計（OOD）— 設計模式的選型與核心邏輯

以「**需求驅動設計**」為主軸，說明每個模式**為什麼被選中**、**解決什麼 OOA 階段識別的問題**、以及**角色如何對應到類別**。

> 參照 OOD 圖：[`docs/OOD/ood-basic.jpg`](OOD/ood-basic.jpg)、[`docs/OOD/ood-bonus.jpg`](OOD/ood-bonus.jpg)
> 參照 UML 類別圖：[`spec/class-diagram.md`](../spec/class-diagram.md)

### 3.1 Composite Pattern — 遞迴樹狀結構的統一操作

**OOA 問題**：目錄可無限嵌套，且目錄與檔案需被統一操作（遍歷、刪除、複製）。

**模式選型理由**：Composite 讓 Client 不需區分葉節點（File）與組合節點（Directory），以統一介面操作整棵樹。

**角色對應**：

| GoF 角色 | 本專案類別 | 職責 |
| -------- | ---------- | ---- |
| Component | `FileSystemComponent` | 抽象基底：定義 `Name`、`GetSize()`、`Accept()`、`DeepCopy()` 統一介面 |
| Composite | `Directory` | 持有 `List<FileSystemComponent> _children`，實作 `Add`/`Remove`/遞迴 `GetSize` |
| Leaf | `File`（abstract）→ `WordDocument`, `ImageFile`, `TextFile` | `GetSize()` 回傳自身 `Size`，`DeepCopy()` 複製自身屬性 |

**核心邏輯**：

- **Parent 自動維護**：`Add()` 設定子元件的 `Parent` 為此目錄，`Remove()` 清除為 `null`。外部無法直接設定 `Parent`（`internal set`），確保關係一致性。
- **對外唯讀**：`Children` 屬性型別為 `IReadOnlyList<FileSystemComponent>`，內部操作僅透過 `Add`/`Remove`/`Insert` 方法。
- **遞迴加總**：`Directory.GetSize()` 以 `_children.Sum(c => c.GetSize())` 遞迴累加，體現 Composite 的核心遞迴結構。
- **防禦性設計**：`IsAncestorOf()` 防止循環參照（將祖先加為自己的子元件）；`IndexOf()` 支援 `DeleteCommand` 記錄精確位置以便 undo 還原。

### 3.2 Visitor Pattern — 將遍歷操作與資料結構解耦

**OOA 問題**：四種唯讀操作（顯示、計算大小、搜尋、XML 輸出）若直接寫在模型類別中，每新增一種操作就要修改所有元件類別，違反開放封閉原則（OCP）。

**模式選型理由**：Visitor 透過雙重分派（Double Dispatch），讓新操作只需新增 Visitor 類別，無需修改現有元件結構。

**角色對應**：

| GoF 角色 | 本專案類別 | 職責 |
| -------- | ---------- | ---- |
| Visitor | `IFileSystemVisitor` | 定義 `Visit()` overloads：`Directory`、`WordDocument`、`ImageFile`、`TextFile` |
| ConcreteVisitor | `DisplayVisitor`, `SizeCalculatorVisitor`, `SearchByExtensionVisitor`, `XmlExportVisitor` | 各自實作一種遍歷操作 |
| Element | `FileSystemComponent` | 定義 `abstract Accept(IFileSystemVisitor)` |
| ConcreteElement | `Directory`, `WordDocument`, `ImageFile`, `TextFile` | 實作 `Accept()` → 呼叫 `visitor.Visit(this)` |

**核心邏輯——雙重分派機制**：

```txt
Client                    Element                   Visitor
  │                         │                         │
  ├─ component.Accept(v) ──→│                         │
  │    第一次分派：          │                         │
  │    由元件的實際型別      │── visitor.Visit(this) ──→│
  │    決定進入哪個 Accept   │    第二次分派：          │
  │                         │    由 Visit overload     │
  │                         │    決定操作邏輯          │
```

- **第一次分派**：Client 呼叫 `component.Accept(visitor)` → 由元件的**實際型別**決定進入哪個 `Accept` 實作
- **第二次分派**：`Accept` 內部呼叫 `visitor.Visit(this)` → 由 `Visit` 的 overload 決定操作邏輯
- **樹走訪**：`Directory.Accept()` 先呼叫 `visitor.Visit(this)` 處理自身，再遞迴呼叫每個 child 的 `Accept`

**四個 Visitor 的職責**：

| Visitor | 功能 | 實作要點 |
| ------- | ---- | -------- |
| `DisplayVisitor` | 以 Box-drawing 字元渲染樹狀結構 | 以 `List<bool> _isLast` 追蹤每層是否為最後一個子元件，決定使用 `├──` 或 `└──` |
| `SizeCalculatorVisitor` | 遞迴累加檔案大小 | 走訪時輸出 Traverse Log（`Visiting: {path}`），結果存於 `TotalSize` 屬性 |
| `SearchByExtensionVisitor` | 依副檔名篩選檔案 | 走訪時輸出 Traverse Log，符合的路徑收集至 `Results` 清單 |
| `XmlExportVisitor` | 將結構序列化為 XML | 以 `_indentLevel` 追蹤縮排深度，`SanitizeTagName()` 處理中文名稱為合法 XML 標籤 |

### 3.3 Command Pattern — 可復原的狀態修改操作管理

**OOA 問題**：刪除、貼上、排序、標籤操作需支援 undo/redo，且操作歷史需有序管理。

**模式選型理由**：Command 將每個狀態修改操作封裝為物件，記錄執行前狀態以支援反向操作。

**角色對應**：

| GoF 角色 | 本專案類別 | 職責 |
| -------- | ---------- | ---- |
| Command | `ICommand` | 介面：`Execute()`、`Undo()`、`Description` |
| ConcreteCommand | `DeleteCommand`, `PasteCommand`, `SortCommand`, `TagCommand`, `UntagCommand` | 各自封裝一種操作與其逆操作 |
| Invoker | `CommandManager` | 持有雙 Stack 管理 undo/redo 歷史 |
| Client | `CloudFileSystemCli` | 建立對應 Command 物件，交給 `CommandManager` 執行 |
| Receiver | `Directory`, `FileSystemComponent` | 實際被操作的領域物件 |

**核心邏輯——雙 Stack 狀態機**：

```txt
Execute(cmd)：cmd.Execute() → push undoStack → clear redoStack
Undo()：      pop undoStack → cmd.Undo() → push redoStack
Redo()：      pop redoStack → cmd.Execute() → push undoStack
```

任何新的 `Execute` 都會清空 `redoStack`，確保不會在分叉的歷史線上重做。

**五個 Command 的狀態保存策略**：

| Command | Execute | Undo | 狀態保存 |
| ------- | ------- | ---- | -------- |
| `DeleteCommand` | `_parent.Remove(_component)` | `_parent.Insert(_originalIndex, _component)` | 建構時記錄 `_originalIndex`，精確還原位置 |
| `PasteCommand` | 首次時 `_clipboard.DeepCopy()` 產生 `_cloned` 後 `_target.Add(_cloned)` | `_target.Remove(_cloned)` | Redo 重用同一 `_cloned`，避免重複深拷貝 |
| `SortCommand` | `_directory.Sort(_sortBy, _sortOrder)` | 遍歷 `_originalOrders` 呼叫 `SetChildrenOrder()` | 建構時 `SnapshotOrder()` 遞迴快照整棵子樹的順序 |
| `TagCommand` | `_component.AddTag(_tag)` | `_component.RemoveTag(_tag)` | 操作本身即為互逆 |
| `UntagCommand` | `_component.RemoveTag(_tag)` | `_component.AddTag(_tag)` | 操作本身即為互逆 |

**設計決策——唯讀 vs 狀態修改的分界**：

- **唯讀操作**（display, size, search, xml）：直接透過 Visitor 執行，不進入 `CommandManager`
- **狀態修改操作**（delete, paste, sort, tag, untag）：一律透過 `CommandManager`，確保 undo/redo 歷史完整

這個分界在 `CloudFileSystemCli.ExecuteCommand()` 中清楚體現：唯讀指令直接建立 Visitor 並呼叫 `Accept()`；狀態修改指令建立 Command 物件後交給 `_commandManager.Execute()`。

### 3.4 Prototype Pattern — 樹狀結構的深拷貝

**OOA 問題**：複製/貼上功能需產生完全獨立的副本，修改副本不影響原件，且需遞迴處理整棵子樹。

**模式選型理由**：Prototype 讓每個元件自行負責克隆邏輯，避免外部程式碼了解內部結構細節。

**角色對應**：

| GoF 角色 | 本專案類別 | 職責 |
| -------- | ---------- | ---- |
| Prototype | `FileSystemComponent` | 定義 `abstract DeepCopy()` |
| ConcretePrototype | `Directory`, `WordDocument`, `ImageFile`, `TextFile` | 各自實作深拷貝邏輯 |
| Client | `PasteCommand` | 呼叫 `_clipboard.DeepCopy()` 建立獨立副本 |

**核心邏輯——遞迴深拷貝**：

```txt
Directory.DeepCopy()
  ├── new Directory(Name)
  ├── CopyTagsTo(copy)           ← 複製標籤
  └── foreach child in _children
      └── copy.Add(child.DeepCopy())  ← 遞迴深拷貝 + Add() 自動維護 Parent
```

- `Directory.DeepCopy()`：建立新 Directory → 遞迴呼叫每個 child 的 `DeepCopy()` → 透過 `Add()` 建立正確的 `Parent` 關係
- `File` 子類別的 `DeepCopy()`：建立新實例，複製所有屬性（`Size`, `CreatedAt`, 型別專屬屬性如 `PageCount`）+ `CopyTagsTo()`

**設計決策——延遲拷貝**：

| 指令 | 行為 | 理由 |
| ---- | ---- | ---- |
| `copy` | 僅存參照到 `_clipboard` | 使用者可能 copy 後改變心意，不需立即付出深拷貝成本 |
| `paste` | 呼叫 `DeepCopy()` 產生獨立副本 | 此時才確定要貼上，執行實際拷貝 |
| `redo`（paste） | 重用首次產生的 `_cloned` | 避免重複深拷貝，且確保名稱一致（含自動改名） |

---

## 四、模式協作全景

以一次完整的「**複製目錄 → 貼到另一個目錄 → 顯示結果 → undo**」操作流程，串聯四個設計模式的協作：

### 步驟一：copy — 存參照

```txt
使用者輸入: copy 專案文件 (Project_Docs)

CLI (Client)
 └── FindChild("專案文件 (Project_Docs)")
      └── _clipboard = component        ← 僅存參照，不涉及任何 Pattern
```

此步驟不建立 Command（非狀態修改操作），不觸發深拷貝。

### 步驟二：paste — Prototype + Command + Composite 協作

```txt
使用者輸入: paste 個人筆記 (Personal_Notes)

CLI (Client)
 ├── new PasteCommand(target, _clipboard)     ← Command Pattern：封裝操作
 └── _commandManager.Execute(command)         ← Invoker 觸發
      └── command.Execute()
           ├── _clipboard.DeepCopy()          ← Prototype Pattern：遞迴深拷貝
           │    └── Directory.DeepCopy()
           │         ├── new Directory("專案文件 (Project_Docs)")
           │         ├── child1.DeepCopy() → new WordDocument(...)
           │         └── child2.DeepCopy() → new ImageFile(...)
           ├── GenerateUniqueName()           ← 同名時自動改名
           └── target.Add(_cloned)            ← Composite Pattern：加入樹結構
                └── _cloned.Parent = target   ← 自動維護 Parent
```

- **Prototype** 負責產生獨立副本
- **Composite** 的 `Add()` 將副本接入樹並維護 `Parent`
- **Command** 的 `CommandManager` 將此操作推入 `_undoStack`

### 步驟三：display — Visitor + Composite 協作

```txt
使用者輸入: display

CLI (Client)
 └── new DisplayVisitor()
      └── _root.Accept(visitor)               ← Composite Pattern：統一介面
           └── visitor.Visit(directory)        ← Visitor Pattern：雙重分派
                ├── 輸出 "根目錄 (Root)"
                └── foreach child.Accept(visitor)  ← 遞迴走訪整棵樹
                     └── visitor.Visit(this)   ← 依實際型別 dispatch
```

- **Composite** 提供統一的 `Accept()` 介面，Client 不需區分目錄或檔案
- **Visitor** 的雙重分派讓 `DisplayVisitor` 針對不同元件類型輸出不同格式

### 步驟四：undo — Command 反向操作

```txt
使用者輸入: undo

CLI (Client)
 └── _commandManager.Undo()
      ├── pop _undoStack → PasteCommand
      ├── command.Undo()
      │    └── target.Remove(_cloned)          ← Composite：移除副本
      │         └── _cloned.Parent = null
      └── push _redoStack                      ← 保留以供 redo
```

- **Command** 的 `Undo()` 呼叫 `Remove()` 移除先前貼上的副本
- **Composite** 的 `Remove()` 自動清除 `Parent` 關係
- `_cloned` 仍保留在 `PasteCommand` 中，若之後 redo 可直接重用

---

## 五、總結

### 從 OOA 到 OOD 的推導路徑

```txt
需求訪談
 ├── 名詞/動詞萃取 → 名詞分類（類別 vs 屬性）
 ├── 實體與關係識別 → 領域模型
 └── 操作分類（唯讀 vs 狀態修改）
      │
      ├── 「統一操作遞迴結構」 ──→ Composite Pattern
      ├── 「解耦遍歷操作」    ──→ Visitor Pattern
      ├── 「可復原的操作」    ──→ Command Pattern
      └── 「深拷貝子樹」      ──→ Prototype Pattern
```

每個設計模式的選用都可追溯到 OOA 階段識別的具體問題，而非先選定模式再套用。

### 設計取捨與權衡

| 取捨 | 選擇 | 理由 |
| ---- | ---- | ---- |
| 簡潔性 vs 擴充性 | 偏向擴充性 | Visitor 讓新增操作不改現有類別；Command 讓新增操作類型自動獲得 undo/redo |
| 延遲拷貝 vs 即時拷貝 | 延遲拷貝 | `copy` 僅存參照，`paste` 時才 `DeepCopy()`，避免不必要的成本 |
| 唯讀/狀態修改分界 | 明確分開 | 唯讀走 Visitor，狀態修改走 Command，職責清晰且避免 undo 歷史膨脹 |
| Visitor 的 OCP 取捨 | 接受新增元件類型需修改 Visitor | 元件類型穩定（檔案類型不常變）；操作種類更可能擴增，Visitor 優化了操作擴增的方向 |

### 開放封閉原則的實踐

- **新增檔案類型**（如 `PdfFile`）：加子類別 + 每個 Visitor 加一個 `Visit` overload，不需修改現有元件邏輯
- **新增唯讀操作**（如統計檔案數量）：加一個新的 `IFileSystemVisitor` 實作，不需修改任何元件類別
- **新增狀態修改操作**（如重新命名）：加一個新的 `ICommand` 實作，自動獲得 `CommandManager` 的 undo/redo 管理
