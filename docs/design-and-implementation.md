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

| 分類         | 名詞                                              | 判斷依據                           |
| ------------ | ------------------------------------------------- | ---------------------------------- |
| **類別候選** | Word 文件、圖片、純文字檔、目錄                   | 具有獨立行為與狀態，能作為操作對象 |
| **屬性候選** | 檔名、大小、建立時間、頁數、解析度（寬/高）、編碼 | 描述某個類別的特徵，不能獨立存在   |

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
| 顯示樹狀結構 | 以 Box-drawing 字元渲染完整樹    | 需走訪整棵樹，且格式化邏輯不應寫在模型中 |
| 計算容量     | 遞迴加總指定目錄下所有檔案大小   | 需走訪整棵子樹 + 輸出 Traverse Log       |
| 搜尋         | 依副檔名篩選，列出符合的檔案路徑 | 需走訪整棵樹 + 輸出 Traverse Log         |
| XML 輸出     | 將結構序列化為 XML 格式          | 需走訪整棵樹，且序列化邏輯不應寫在模型中 |

**共同特徵**：都需要走訪樹的節點、都是在結構上執行不同的「操作」、都不應讓模型類別承擔格式化責任。
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

**小結**：OOA 階段識別的三個核心問題——「統一操作遞迴結構」、「解耦走訪操作與資料結構」、「可復原的突變操作」——分別對應了 Composite、Visitor、Command 三個設計模式的選用，再加上深拷貝需求引入的 Prototype，構成了本專案的四個設計模式。

---

## 三、物件導向設計（OOD）— 以 Pattern 框架推導設計結構

本章以 **Pattern 框架**（Context → Forces → Problem → Solution → Resulting Context）為敘事結構，搭配**依賴反轉之重構三步驟**（封裝變動之處 → 萃取共同行為 → 委派/複合）推導每個設計模式的設計結構。四個模式之間以 Resulting Context → Context 的銜接形成連續的設計推導鏈。

> 參照 OOD 圖：[`docs/OOD/ood-basic.jpg`](OOD/ood-basic.jpg)、[`docs/OOD/ood-bonus.jpg`](OOD/ood-bonus.jpg)
> 參照 UML 類別圖：[`spec/class-diagram.md`](../spec/class-diagram.md)

### 3.1 Composite Pattern — 遞迴樹狀結構的統一操作

#### Context

OOA 階段識別出目錄與檔案構成遞迴樹狀結構——目錄可無限嵌套子目錄，且多種操作（顯示、刪除、複製、計算大小）需統一作用於目錄和檔案。領域模型已建立（`FileSystemComponent` → `Directory` / `File` → 三種具體檔案類型），但尚未定義結構的擴充策略。

#### Forces

- **高結構複雜度（High Structural Complexity）**：結構中存在遞迴自我關聯（Self Association），目錄的 children 可以是檔案也可以是子目錄，形成無限深度的樹狀結構。外部操作若需區分目錄與檔案分別處理，邏輯將極度複雜。
- **結構變動性（Structural Variation）**：未來需求可能需要支援新的檔案類型（如 PDF、音訊檔）。若每新增一種檔案類型就需修改操作端程式碼，維護成本隨類型數量線性增長。
- **開放封閉原則（OCP）**：擴充新的檔案類型時，應有最少的修改成本——理想狀態是只加新類別，不改既有程式碼。

#### Problem

如何讓 CloudFileSystem 的樹狀結構有良好的擴充性？並且在擴充時不影響到 CLI 類別的程式？

#### Solution — 依賴反轉之重構三步驟

1. **封裝變動之處**：將各檔案類型（WordDocument、ImageFile、TextFile）和目錄（Directory）各自封裝為獨立類別，隱藏各自的專屬屬性與行為。每個類別只暴露外部需要的介面，內部實作細節不可見。
2. **萃取共同行為**：識別所有元件的共同行為（`GetSize()`、`Accept()`、`DeepCopy()`）與共同屬性（`Name`、`Parent`），萃取為抽象基底類別 `FileSystemComponent`。
3. **委派/複合**：Directory 持有 `List<FileSystemComponent>`（Composition），透過 Component 抽象管理子元件。CLI 透過 `FileSystemComponent` 抽象操作整棵樹，不需知道具體型別 → **依賴反轉**：CLI 不依賴具體檔案類別，而是依賴抽象的 `FileSystemComponent`。

**角色對應**：

| GoF 角色  | 本專案類別                                                  | 職責                             |
| --------- | ----------------------------------------------------------- | -------------------------------- |
| Component | `FileSystemComponent`                                       | 抽象基底：定義所有元件的統一介面 |
| Composite | `Directory`                                                 | 持有子元件清單，實作遞迴操作     |
| Leaf      | `File`（abstract）→ `WordDocument`, `ImageFile`, `TextFile` | 葉節點，不含子元件               |

#### Resulting Context

- **好處**：CLI 以統一介面操作整棵樹，新增檔案類型只需加子類別，不影響 CLI 及其他既有程式碼。
- **取捨**：Leaf 繼承了 Component 的所有介面，部分方法（如 `Add`/`Remove`）對 Leaf 無意義，需在設計上處理這個語意落差。
- **銜接**：Composite 結構建立後，需要在結構上執行多種不同的操作（顯示、計算大小、搜尋、XML 輸出）。若將這些操作邏輯直接寫在各 Element 類別中，每新增一種操作就必須修改所有 Element 類別 → 引出 **Visitor Pattern**。

### 3.2 Visitor Pattern — 將走訪操作與資料結構解耦

#### Context

Composite 結構已建立，現在需要對此結構執行多種唯讀操作（顯示、計算大小、搜尋、XML 輸出），且這些操作未來會持續擴充。問題在於：這些操作的邏輯應該放在哪裡？

#### Forces

- **高結構複雜度（High Structural Complexity）**：結構中存在遞迴關聯（Self Association）的樹狀結構。
- **操作變動性（Operational Variation）**：需要對樹狀結構中的元素執行多種不同的操作，好比：生成目錄結構、以 XML 結構輸出、計算大小或搜尋時紀錄走訪過程（Traverse Log）。且未來會持續擴充新的操作。
- **開放封閉原則（OCP）**：在擴充新的操作時，可以不必修改任何既有的 Element 類別程式碼。既有的 Element 類別應該對擴充開放，但對修改關閉。

#### Problem

如何讓開發人員能夠持續對樹狀結構中的各類元素擴充新的操作，並且將每一種操作的邏輯集中封裝在同一個類別中，而完全不必修改既有的核心結構 Element 類別？

#### Solution — 依賴反轉之重構三步驟

1. **封裝變動之處**：將每種操作的邏輯從 Element 類別中抽離，各自封裝為獨立的 Visitor 類別（`DisplayVisitor`、`SizeCalculatorVisitor`、`SearchByExtensionVisitor`、`XmlExportVisitor`）。每個 Visitor 集中管理一種操作對所有元件類型的處理邏輯。
2. **萃取共同行為**：識別所有 Visitor 的共同行為——對各種 Element 執行 Visit，萃取為 `IFileSystemVisitor` 介面，定義針對每種 Element 類型的 `Visit()` overload。
3. **委派**：Element 透過 `Accept(IFileSystemVisitor)` 將操作委派給外部 Visitor。`Accept` 內部呼叫 `visitor.Visit(this)` 形成**雙重分派（Double Dispatch）**——第一次分派由元件的實際型別決定進入哪個 `Accept` 實作，第二次分派由 `Visit` 的 overload 決定操作邏輯 → **依賴反轉**：Element 不依賴具體操作，而是依賴 Visitor 抽象。

**角色對應**：

| GoF 角色        | 本專案類別                                                                                | 職責                                       |
| --------------- | ----------------------------------------------------------------------------------------- | ------------------------------------------ |
| Visitor         | `IFileSystemVisitor`                                                                      | 定義對各種 Element 的 `Visit()` overloads  |
| ConcreteVisitor | `DisplayVisitor`, `SizeCalculatorVisitor`, `SearchByExtensionVisitor`, `XmlExportVisitor` | 各自封裝一種走訪操作的完整邏輯             |
| Element         | `FileSystemComponent`                                                                     | 定義 `abstract Accept(IFileSystemVisitor)` |
| ConcreteElement | `Directory`, `WordDocument`, `ImageFile`, `TextFile`                                      | 實作 `Accept()`，將自身分派給 Visitor      |

**四個 Visitor 的功能**：

| Visitor                    | 功能                                      |
| -------------------------- | ----------------------------------------- |
| `DisplayVisitor`           | 以 Box-drawing 字元 render 樹狀結構       |
| `SizeCalculatorVisitor`    | 遞迴累加檔案大小，走訪時輸出 Traverse Log |
| `SearchByExtensionVisitor` | 依副檔名篩選檔案，走訪時輸出 Traverse Log |
| `XmlExportVisitor`         | 將結構序列化為 XML 格式                   |

**核心機制——雙重分派（Double Dispatch）**：

```txt
Element.Accept(visitor)     → 第一次分派：由元件實際型別決定進入哪個 Accept
  └─ visitor.Visit(this)    → 第二次分派：由 Visit overload 決定操作邏輯
```

- 兩次分派的結合，使得正確的操作邏輯能在「不知道元件具體型別」的情況下被觸發
- `Directory.Accept()` 額外遞迴呼叫每個 child 的 `Accept()`，驅動整棵樹的走訪

#### Resulting Context

- **好處**：新增操作只需加 Visitor 類別，不修改任何 Element；每種操作的邏輯集中在單一類別中，易於維護。
- **取捨**：新增 Element 類型時需修改所有 Visitor（加 `Visit` overload）。但本系統中元件類型穩定，操作種類更可能擴增，Visitor 改善了正確的變動方向。
- **銜接**：Visitor 處理唯讀操作，但系統還需支援狀態修改操作（刪除、貼上、排序、標籤/取消標籤），且這些操作需支援 undo/redo。每個操作的逆邏輯各不相同，需要統一的抽象來管理 → 引出 **Command Pattern**。

### 3.3 Command Pattern — 可復原的狀態修改操作管理

#### Context

系統除了 Visitor 處理的唯讀操作外，還需支援狀態修改操作（刪除、貼上、排序、標籤/取消標籤），且所有狀態修改操作需支援 undo/redo。設計決策上，唯讀操作直接透過 Visitor 執行，不進入歷史管理；狀態修改操作則需要統一的歷史追蹤機制。

#### Forces

- **操作可逆性（Operation Reversibility）**：使用者需要對已執行的操作進行復原與重做，而每個操作的逆邏輯各不相同（delete 的逆是 insert-back、sort 的逆是還原原始順序、tag 的逆是 untag），必須有統一的抽象來封裝這些差異。
- **操作歷史追蹤（History Tracking）**：Undo/Redo 需要維護有序的操作歷史，支援任意深度的連續復原與重做。若沒有統一的 Command 抽象，歷史管理器無法以泛用方式管理不同類型的操作。

#### Problem

如何將各種改變狀態的操作及其逆操作封裝為統一的抽象，使歷史管理機制能以泛用方式管理任意操作的執行、復原與重做？

#### Solution — 依賴反轉之重構三步驟

1. **封裝變動之處**：將每個狀態修改操作（Delete、Paste、Sort、Tag、Untag）及其逆操作各自封裝為獨立的 Command 物件。每個 Command 自行負責保存執行前狀態以支援 Undo。
2. **萃取共同行為**：識別所有 Command 的共同能力（`Execute()` + `Undo()`），萃取為 `ICommand` 介面。
3. **委派**：CLI（Client）建立具體 Command 後交給 `CommandManager`（Invoker）。Invoker 只依賴 `ICommand` 介面，透過雙 Stack 管理執行/復原/重做 → **依賴反轉**：Invoker 不依賴具體操作類別，而是依賴 `ICommand` 抽象。

**角色對應**：

| GoF 角色        | 本專案類別                                                                   | 職責                                  |
| --------------- | ---------------------------------------------------------------------------- | ------------------------------------- |
| Command         | `ICommand`                                                                   | 定義 `Execute()` 與 `Undo()` 統一介面 |
| ConcreteCommand | `DeleteCommand`, `PasteCommand`, `SortCommand`, `TagCommand`, `UntagCommand` | 各自封裝一種操作與其逆操作            |
| Invoker         | `CommandManager`                                                             | 持有雙 Stack，管理 undo/redo 歷史     |
| Client          | `CloudFileSystemCli`                                                         | 建立 Command 物件，交給 Invoker 執行  |
| Receiver        | `Directory`, `FileSystemComponent`                                           | 實際被操作的領域物件                  |

**核心機制——雙 Stack 狀態機**：

```txt
Execute(cmd)：cmd.Execute() → push undoStack → clear redoStack
Undo()：      pop undoStack → cmd.Undo()    → push redoStack
Redo()：      pop redoStack → cmd.Execute() → push undoStack
```

任何新的 `Execute` 都會清空 `redoStack`，確保不會在分叉的歷史線上重做。

**五個 Command 的狀態保存策略**：

| Command         | 逆操作策略     | 狀態保存                                    |
| --------------- | -------------- | ------------------------------------------- |
| `DeleteCommand` | 還原至原位置   | 記錄原始 index                              |
| `PasteCommand`  | 移除貼上的副本 | 首次 Execute 時 DeepCopy，Redo 重用同一副本 |
| `SortCommand`   | 還原排序前順序 | 遞迴快照整棵子樹的 children 順序            |
| `TagCommand`    | 移除標籤       | 操作本身即互逆                              |
| `UntagCommand`  | 加回標籤       | 操作本身即互逆                              |

#### Resulting Context

- **好處**：新增操作只需加 Command 類別即自動獲得 undo/redo 支援；Invoker 與 Receiver 完全解耦，歷史管理邏輯集中在 `CommandManager`。
- **取捨**：每個 Command 需自行設計狀態保存策略（記錄哪些執行前狀態、如何還原），增加了各 Command 的實作責任。
- **銜接**：`PasteCommand` 需要對 Composite 樹進行深拷貝以產生獨立副本（修改副本不影響原件）。由於 Directory 的 children 可能是任何子型別，且結構存在遞迴自我關聯與雙向參照，深拷貝邏輯不能由外部硬編碼 → 引出 **Prototype Pattern**。

### 3.4 Prototype Pattern — 樹狀結構的深拷貝

#### Context

Command Pattern 的 `PasteCommand` 需要對 Composite 樹狀結構進行深拷貝，以產生完全獨立的副本（修改副本不影響原件）。目前系統尚未有機制來完成這件事。

#### Forces

- **動態建立（Dynamic Creation）**：執行深拷貝時，被複製的節點其具體類別（WordDocument、ImageFile、TextFile、Directory）只有在執行期才能確定，編譯期無法得知。
- **複雜初始化（Complex Initialization）**：若被複製的節點是 Directory，深拷貝必須遞迴走訪並複製其內部所有子節點（子節點本身也可能是 Directory），才能確保副本與原樹不共享任何參照。同時每個節點透過 `Parent` 屬性反向引用父節點，構成雙向參照，新子樹內部的所有 `Parent` 必須正確指向新建立的父節點。結構越深越廣，初始化過程就越繁瑣。
- **狀態保留（State Retention）**：深拷貝產生的副本必須攜帶原物件的完整狀態與結構，而非從空白狀態開始——名稱、標籤、各子型別的專屬屬性（PageCount、Width/Height、Encoding），以及 Directory 內部整棵子樹的組成。物件的內部結構可能很複雜，外部根本無法完整地重建其內部狀態。

三條 Force 合在一起，把所有其他建立物件的方式都排除了——不能直接 `new`（Dynamic Creation）、不能從頭建構（Complex Initialization）、不能建立空白物件再逐一設值（State Retention）。唯一剩下的路就是讓物件自己複製自己，因為**只有物件本身同時擁有關於自己具體類別、初始化結果、與完整內部狀態的知識**。

#### Problem

如何對 Composite 樹狀結構中的任意節點進行深拷貝，在不依賴具體類別的前提下，產生一棵保留完整狀態且與原樹完全獨立的副本？

#### Solution — 依賴反轉之重構三步驟

1. **封裝變動之處**：讓每個 ConcretePrototype（Directory、WordDocument、ImageFile、TextFile）各自封裝自己的克隆邏輯，自行負責複製專屬屬性（如 PageCount、Width/Height、Encoding）與標籤。
2. **萃取共同行為**：識別共同能力——自我複製，萃取為 `FileSystemComponent.DeepCopy()` 抽象方法。
3. **委派**：`PasteCommand` 只呼叫 `clipboard.DeepCopy()`，由多型決定實際克隆行為 → **依賴反轉**：PasteCommand 不依賴具體檔案類別的建構邏輯，而是依賴 `FileSystemComponent` 的 `DeepCopy()` 抽象。

**角色對應**：

| GoF 角色          | 本專案類別                                           | 職責                           |
| ----------------- | ---------------------------------------------------- | ------------------------------ |
| Prototype         | `FileSystemComponent`                                | 定義 `abstract DeepCopy()`     |
| ConcretePrototype | `Directory`, `WordDocument`, `ImageFile`, `TextFile` | 各自實作完整的深拷貝邏輯       |
| Client            | `PasteCommand`                                       | 呼叫 `DeepCopy()` 建立獨立副本 |

**設計決策——延遲拷貝**：`copy` 指令僅將參照存入剪貼簿，不觸發深拷貝；`paste` 時才呼叫 `DeepCopy()` 產生獨立副本，避免使用者 copy 後改變心意時付出不必要的成本。Redo 時重用首次產生的副本，避免重複深拷貝且確保名稱一致（含自動改名結果）。

**核心機制——遞迴深拷貝**：

```txt
Directory.DeepCopy()
  → 建立新 Directory → 複製標籤
  → 對每個 child 呼叫 child.DeepCopy()
  → 透過 Add() 加入新 Directory（自動維護 Parent 指向新父節點）
```

每個 File 子類別的 `DeepCopy()` 建立新實例並複製所有屬性（Size、CreatedAt、型別專屬屬性）與標籤。整棵新子樹的 `Parent` 關係完全獨立於原始結構。

#### Resulting Context

- **好處**：新增檔案類型只需實作自己的 `DeepCopy()`，`PasteCommand` 完全不需修改。每個元件自行確保深拷貝的完整性，外部呼叫端無需了解內部結構。
- **取捨**：每個 ConcretePrototype 需自行確保深拷貝的完整性（包含標籤、專屬屬性、遞迴子樹），若遺漏某個屬性將導致副本不完整。

---

## 四、模式協作全景

以一次完整的「**複製目錄 → 貼到另一個目錄 → 顯示結果 → undo**」操作流程，說明四個設計模式如何協作。每個步驟聚焦於「哪個模式負責什麼」。

### 步驟一：copy — 存參照

使用者輸入 `copy 專案文件 (Project_Docs)`。CLI 在 Composite 結構中找到目標元件，將參照存入剪貼簿。此步驟不建立 Command（非狀態修改操作），不觸發深拷貝——這是延遲拷貝策略的體現。

### 步驟二：paste — Prototype + Command + Composite 協作

使用者輸入 `paste 個人筆記 (Personal_Notes)`。三個模式在此步驟協作：

1. **Command Pattern**：CLI 建立 `PasteCommand` 並交給 `CommandManager` 執行，操作被推入 undo 歷史
2. **Prototype Pattern**：`PasteCommand.Execute()` 呼叫 `clipboard.DeepCopy()` 遞迴深拷貝整棵子樹，產生完全獨立的副本
3. **Composite Pattern**：透過 `Directory.Add()` 將副本接入目標目錄的樹結構，自動維護 `Parent` 參照

### 步驟三：display — Visitor + Composite 協作

使用者輸入 `display`。兩個模式協作：

1. **Composite Pattern**：提供統一的 `Accept()` 介面，CLI 不需區分目錄或檔案，對根節點呼叫 `Accept()` 即可
2. **Visitor Pattern**：`DisplayVisitor` 透過雙重分派，針對不同元件類型輸出不同格式，遞迴走訪整棵樹

此步驟為唯讀操作，直接透過 Visitor 執行，不進入 `CommandManager`。

### 步驟四：undo — Command 反向操作

使用者輸入 `undo`。`CommandManager` 從 undo 歷史中取出 `PasteCommand`，呼叫其 `Undo()` 方法。`PasteCommand.Undo()` 透過 Composite 的 `Remove()` 將先前貼上的副本從樹結構中移除，`Parent` 關係自動清除。已移除的副本仍保留在 `PasteCommand` 中，若之後 redo 可直接重用，無需再次深拷貝。

---

## 五、總結

### 從 OOA 到 OOD 的設計推導鏈

```txt
需求訪談
 ├── 名詞/動詞萃取 → 名詞分類（類別 vs 屬性）
 ├── 實體與關係識別 → 領域模型
 └── 操作分類（唯讀 vs 狀態修改）
      │
      ├── 「統一操作遞迴結構」 ──→ Composite Pattern
      │                              ↓ Resulting Context
      ├── 「解耦走訪操作」    ──→ Visitor Pattern
      │                              ↓ Resulting Context
      ├── 「可復原的操作」    ──→ Command Pattern
      │                              ↓ Resulting Context
      └── 「深拷貝子樹」      ──→ Prototype Pattern
```

每個設計模式的選用都可追溯到 OOA 階段識別的具體問題，而非先選定模式再套用。四個模式之間以 Resulting Context → Context 的銜接，形成一條連續的設計推導鏈——前一個模式的結果自然引出下一個模式的問題。

### 設計取捨與權衡

| 取捨                 | 選擇         | 理由                                                                             |
| -------------------- | ------------ | -------------------------------------------------------------------------------- |
| 簡潔性 vs 擴充性     | 偏向擴充性   | Visitor 讓新增操作不改現有類別；Command 讓新增操作自動獲得 undo/redo             |
| 延遲拷貝 vs 即時拷貝 | 延遲拷貝     | `copy` 僅存參照，`paste` 時才 `DeepCopy()`，避免不必要的成本                     |
| 唯讀/狀態修改分界    | 明確分開     | 唯讀走 Visitor，狀態修改走 Command，職責清晰且避免 undo 歷史膨脹                 |
| Visitor 的 OCP 方向  | 改善操作擴增 | 元件類型穩定（檔案類型不常變）；操作種類更可能擴增，Visitor 改善了正確的變動方向 |

### 開放封閉原則的實踐

- **新增檔案類型**（如 `PdfFile`）：加子類別 + 實作 `DeepCopy()` + 每個 Visitor 加一個 `Visit` overload，不需修改現有元件邏輯
- **新增唯讀操作**（如統計檔案數量）：加一個新的 `IFileSystemVisitor` 實作，不需修改任何 Element 類別
- **新增狀態修改操作**（如重新命名）：加一個新的 `ICommand` 實作，自動獲得 `CommandManager` 的 undo/redo 管理
