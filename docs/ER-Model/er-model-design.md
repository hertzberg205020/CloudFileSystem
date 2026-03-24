# RDBMS schema 設計討論

**在關聯式資料庫（RDBMS）中表達物件導向繼承階層，業界公認有三大核心策略：Single Table Inheritance（STI）、Class Table Inheritance（CTI）與 Concrete Table Inheritance。** 每種策略在查詢效能、資料完整性、正規化程度上各有取捨——沒有「最佳方案」，只有最適合應用場景。

## 分析與定義問題

### Context

根據系統設計，透過 Composite Pattern 將檔案管理系統建模為遞迴樹狀結構。抽象基底類別 `FileSystemComponent` 統一了兩條繼承分支：

+ Composite 分支： `Directory`（持有 children，可無限嵌套）
+ Leaf 分支：File（抽象）← `WordDocument`, `ImageFile`, `TextFile`

現在需要將這個物件導向的繼承階層映射到 RDBMS schema。

三種檔案共用屬性（`Name`、`Size`、`CreatedAt`），各自又有專屬屬性（`PageCount`、`Width`/`Height`、`Encoding`）。Directory 目前只有 `Name`，但作為獨立概念存在於領域模型中。
現在需要將這個物件導向的繼承階層映射到 RDBMS schema，而系統預期會持續演化——未來將支援 PDF、音訊檔等新的檔案類型。

### Forces

**Force 1 — 結構變動性（Structural Variation）**：系統未來需要支援更多檔案類型（如 PDF、音訊檔），每種新類型帶有無法預見的專屬屬性。擴充新類型是確定會反覆發生的事。

**Force 2 — 開放封閉原則（OCP）**：對既有的核心 table 不要變更 schema（closed for modification），同時要能支援新的檔案類型（open for extension）。新增類型的理想操作是「只加新東西，不改舊東西」。

這兩個 force 彼此衝突：系統必須持續演化以容納新類型，但演化的過程又不能動到既有的穩定結構。

### Problem

如何設計 RDBMS schema 來映射 `FileSystemComponent` 的繼承階層，使得未來擴充新檔案類型時，既有核心 table 的 schema 完全不需要變更？

## Solution (Form)

採用 **Class Table Inheritance（CTI）** 策略。繼承階層中的每個類別各自對應一張表，父表存共用屬性，子表只存專屬屬性，透過 PK/FK 關聯。

### Component 層 — 所有元件的統一根表

```sql
CREATE TABLE FileSystemComponent (
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
    ComponentType   NVARCHAR(50) NOT NULL,
    Name            NVARCHAR(255) NOT NULL,
    ParentId        BIGINT,
    FOREIGN KEY (ParentId)
        REFERENCES FileSystemComponent(Id)
);
```

`ParentId` 自我參照實現 Composite Pattern 的遞迴樹狀結構。`ComponentType` 作為鑑別欄位區分 Directory 與各種 File。

### Composite 層 — Directory 子表

```sql
CREATE TABLE Directory (
    Id  BIGINT PRIMARY KEY,
    FOREIGN KEY (Id)
        REFERENCES FileSystemComponent(Id) ON DELETE CASCADE
);
```

目前 Directory 無額外專屬屬性，但建表是為了在結構上預留擴充空間（如未來加入 `PermissionLevel`、`MaxCapacity`），確保屆時只需 ALTER 此表而非父表。

### Leaf 中間層 — File 共用屬性

```sql
CREATE TABLE [File] (
    Id          BIGINT PRIMARY KEY,
    [Size]      BIGINT NOT NULL,
    CreatedAt   DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (Id)
        REFERENCES FileSystemComponent(Id) ON DELETE CASCADE
);
```

承載所有檔案類型共用但 Directory 不具備的屬性，避免這些欄位汙染 Component 層的父表。

### Leaf 具體層 — 各檔案類型專屬屬性

```sql
CREATE TABLE WordDocument (
    Id          BIGINT PRIMARY KEY,
    PageCount   INT NOT NULL,
    FOREIGN KEY (Id) REFERENCES [File](Id) ON DELETE CASCADE
);

CREATE TABLE ImageFile (
    Id      BIGINT PRIMARY KEY,
    Width   INT NOT NULL,
    Height  INT NOT NULL,
    FOREIGN KEY (Id) REFERENCES [File](Id) ON DELETE CASCADE
);

CREATE TABLE TextFile (
    Id        BIGINT PRIMARY KEY,
    Encoding  VARCHAR(20) NOT NULL,
    FOREIGN KEY (Id) REFERENCES [File](Id) ON DELETE CASCADE
);
```

### 應對 Force 的方式

#### Force 1 的應對方式

未來新增支援 PDF 類型時，只需執行

```sql
CREATE TABLE PdfFile (
    Id            BIGINT PRIMARY KEY,
    PageCount     INT NOT NULL,
    IsEncrypted   BIT DEFAULT 0,
    FOREIGN KEY (Id) REFERENCES [File](Id) ON DELETE CASCADE
);
```

#### Force 2 的應對方式

`FileSystemComponent`、`[File]`、`Directory`、`WordDocument`、`ImageFile`、`TextFile` 這些既有表的結構從頭到尾不變。新增檔案類型只需新增一張專屬子表，完全不動既有表的 schema。

### Resulting Context

**正面結果：**

+ 新增檔案類型只需 `CREATE TABLE` 一張新的子表，既有所有表的 schema 完全不變，徹底滿足 OCP。
+ 每張子表可對專屬欄位設定完整的 `NOT NULL`、`UNIQUE` 等約束，資料完整性由資料庫層級保證。
+ 父表 `FileSystemComponent` 的 `ParentId` 自我參照，讓 Composite Pattern 的遞迴樹狀結構在資料庫層直接表達，Directory 和任何 File 類型都共用同一張父表，多型外鍵自然成立。
+ Schema 語意清晰，每張表自我描述——看到 `ImageFile` 就知道圖片有哪些專屬屬性，看到 `Directory` 就知道目錄是一個獨立的概念。

**需要承受的取捨：**

+ 查詢特定檔案類型至少需要兩次 JOIN（如 `FileSystemComponent JOIN [File] JOIN WordDocument`），多型查詢需要 LEFT JOIN 所有子表，JOIN 數量隨子類別增長而線性增加。
+ INSERT/UPDATE 需要操作多張表（寫入一個 WordDocument 要 INSERT 三張表），必須包在 transaction 中確保一致性，寫入效能不如 STI。
+ 整體 schema 的表數量較多，遷移管理成本比 STI 高。

## 另外 2 種策略的簡要分析

### STI（Single Table Inheritance）— 不合適

STI 把所有子類別塞進同一張表。當新增 PDF 檔案類型時，必須對這張**唯一的核心表** `ALTER TABLE ADD COLUMN` 加入 `IsEncrypted` 等欄位。

+ **對 Force 1 的回應**：可以支援新類型，但每次都要修改同一張表的 schema。
+ **對 Force 2 的回應**：**直接違反 OCP**。每新增一種檔案類型，核心表就要被 ALTER 一次，既有欄位越來越多 NULL，表越來越寬越稀疏。

結論：兩個 force 都無法被妥善平衡，排除。

### Concrete Table Inheritance — 不合適

每個具體類別各自一張表，父類別不存在對應的表。新增 PDF 時確實只需 `CREATE TABLE PdfFile (...)`，看似符合 OCP。但問題出在：

+ **對 Force 1 的回應**：可以新增，但父類別的共用欄位（`Name`、`Size`、`CreatedAt`）在每張表中**重複定義**。如果未來需要對共用屬性做任何調整（例如加一個 `UpdatedAt`），就必須 ALTER **所有**子表——這跟 OCP 背道而馳。
+ **對 Force 2 的回應**：表面上新增不動舊表，但「不存在父表」意味著 **無法建立外鍵指向抽象的 File**。而在 Composite Pattern 的結構中，Directory 的 `children` 需要參考任意 `FileSystemComponent`——沒有父表就無法在資料庫層表達這個多型關聯，破壞了整體設計的完整性。

結論：無法支撐 Composite Pattern 的多型外鍵需求，且共用屬性變更會波及所有表，排除。
