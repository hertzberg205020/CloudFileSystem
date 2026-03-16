---
name: csharp-xml-doc-comments
description: "C# XML 文件註解最佳實務與完整標籤指南。基於微軟官方 Recommended XML tags for C# documentation comments，涵蓋所有建議標籤（summary、remarks、param、returns、exception、example、see、inheritdoc 等）、格式化標籤（list、code、para）、連結與參照（cref/href）、泛型標籤（typeparam）、以及文件註解的撰寫原則與常見反模式。當使用者要求加註解、寫文件、補上 XML doc comments、討論該怎麼寫 summary、想知道某個 XML 標籤怎麼用、或在 code review 時討論註解品質時，都應觸發此 skill。即使使用者只是說「加 summary」、「補註解」、「這個方法要怎麼寫文件」、「XML doc 怎麼寫」、「加 /// 註解」，也應觸發。"
---

# C# XML 文件註解最佳實務指南

本指南基於微軟官方 [Recommended XML tags for C# documentation comments](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags)，規範如何為 C# 程式碼撰寫高品質的 XML 文件註解。

---

## 核心原則

微軟官方建議遵循以下原則：

1. **所有公開可見的型別及其公開成員都應加文件註解** — 這是最低標準
2. **每個型別和成員至少要有 `<summary>` 標籤**
3. **用完整句子撰寫，以句號結尾**
4. **XML 必須格式正確** — 否則編譯器會發出警告

XML 文件註解以 `///` 開頭。編譯器會驗證 XML 格式、`<param>` 參數名稱是否存在、以及 `cref` 屬性參照的程式元素是否存在。

---

## 通用標籤（General Tags）

適用於任何型別或成員，是文件註解的基礎。

### `<summary>` — 必備標籤

簡要描述型別或成員的用途。這段文字會顯示在 IntelliSense 和 Object Browser 中，是使用者最先看到的資訊，所以應該精簡但完整地傳達「這是什麼」和「做什麼」。

```xml
<summary>description</summary>
```

```csharp
/// <summary>
/// 代表雲端檔案系統中的一個目錄，可包含子目錄與檔案。
/// </summary>
public class Directory : FileSystemComponent
```

### `<remarks>` — 補充說明

當 `<summary>` 不足以表達完整資訊時，用 `<remarks>` 提供更詳細的說明，例如使用情境、注意事項、設計考量。出現在 Object Browser 中。

```xml
<remarks>
description
</remarks>
```

```csharp
/// <summary>
/// 以遞迴方式計算此目錄下所有檔案的大小總和。
/// </summary>
/// <remarks>
/// <para>此方法會走訪整棵子樹，包含所有巢狀子目錄。</para>
/// <para>空目錄回傳 0。計算結果不包含目錄本身的大小。</para>
/// </remarks>
public long CalculateTotalSize()
```

---

## 成員標籤（Member Tags）

用於描述方法的參數、回傳值、例外與屬性值。

### `<param>` — 描述參數

每個參數都應有對應的 `<param>` 標籤。編譯器會驗證 `name` 屬性是否與方法簽章中的參數名稱一致 — 名稱不符或遺漏參數都會產生警告。

```xml
<param name="name">description</param>
```

```csharp
/// <summary>
/// 在指定的副檔名中搜尋符合的檔案。
/// </summary>
/// <param name="extension">要搜尋的副檔名，包含點號（例如 ".docx"）。</param>
/// <param name="recursive">若為 <see langword="true"/>，則遞迴搜尋子目錄。</param>
public List<File> SearchByExtension(string extension, bool recursive)
```

### `<paramref>` — 在文字中參照參數

在 `<summary>` 或 `<remarks>` 中提到某個參數時，用 `<paramref>` 標記，讓文件工具能以粗體或斜體等方式區別顯示。

```xml
<paramref name="name"/>
```

```csharp
/// <summary>
/// 若 <paramref name="extension"/> 為 <see langword="null"/>，回傳所有檔案。
/// </summary>
```

### `<returns>` — 描述回傳值

描述方法的回傳值代表什麼。值會顯示在 IntelliSense 中。

```xml
<returns>description</returns>
```

```csharp
/// <summary>
/// 計算此目錄下所有檔案的大小總和。
/// </summary>
/// <returns>所有檔案大小的總和（單位：KB）。空目錄回傳 0。</returns>
public long CalculateTotalSize()
```

### `<exception>` — 描述可能拋出的例外

說明方法在什麼條件下拋出什麼例外。`cref` 屬性必須指向存在的例外型別 — 編譯器會驗證。

```xml
<exception cref="member">description</exception>
```

```csharp
/// <summary>
/// 將元件加入此目錄。
/// </summary>
/// <param name="component">要加入的檔案系統元件。</param>
/// <exception cref="ArgumentNullException">
/// <paramref name="component"/> 為 <see langword="null"/> 時拋出。
/// </exception>
/// <exception cref="InvalidOperationException">
/// 當 <paramref name="component"/> 已存在於另一個目錄中時拋出。
/// </exception>
public void Add(FileSystemComponent component)
```

### `<value>` — 描述屬性值

描述屬性所代表的值。顯示在 IntelliSense 中。

```xml
<value>property-description</value>
```

```csharp
/// <summary>
/// 取得此元件的名稱。
/// </summary>
/// <value>檔案或目錄的名稱，不包含路徑。</value>
public string Name { get; }
```

---

## 格式化標籤（Format Tags）

控制文件輸出的格式與結構。

### `<para>` — 段落

在 `<summary>`、`<remarks>` 或 `<returns>` 中分段。`<para>` 會產生雙倍行距的段落。

```csharp
/// <remarks>
/// <para>第一段：說明基本用法。</para>
/// <para>第二段：說明進階情境。</para>
/// </remarks>
```

### `<br/>` — 換行

插入單倍行距的換行，適合不需要段落間距的場合。

```csharp
/// <summary>
/// 支援的格式：<br/>
/// - Word 文件 (.docx)<br/>
/// - 純文字 (.txt)<br/>
/// - 圖片 (.png, .jpg)
/// </summary>
```

### `<c>` — 行內程式碼

標記行內的程式碼片段。

```csharp
/// <summary>
/// 回傳 <c>true</c> 表示目錄為空。
/// </summary>
```

### `<code>` — 程式碼區塊

標記多行程式碼。通常搭配 `<example>` 使用。

```csharp
/// <example>
/// 建立目錄結構並計算大小：
/// <code>
/// var root = new Directory("Root");
/// root.Add(new TextFile("readme.txt", 10, DateTime.Now, "UTF-8"));
/// var totalSize = root.CalculateTotalSize(); // 10
/// </code>
/// </example>
```

### `<example>` — 使用範例

提供方法或類別的使用範例，幫助使用者理解如何呼叫。通常包含 `<code>` 區塊。

```csharp
/// <summary>
/// 使用 Visitor 走訪檔案系統樹狀結構。
/// </summary>
/// <param name="visitor">要套用的 Visitor 實例。</param>
/// <example>
/// 顯示目錄結構：
/// <code>
/// var visitor = new DisplayVisitor(console);
/// root.Accept(visitor);
/// </code>
/// </example>
public abstract void Accept(IFileSystemVisitor visitor);
```

### `<list>` — 列表與表格

支援三種類型：`bullet`（項目符號）、`number`（編號）、`table`（表格）。

```csharp
/// <remarks>
/// 支援的 Visitor 類型：
/// <list type="bullet">
/// <item>
/// <term>DisplayVisitor</term>
/// <description>以樹狀結構顯示檔案系統。</description>
/// </item>
/// <item>
/// <term>SizeCalculatorVisitor</term>
/// <description>計算總檔案大小。</description>
/// </item>
/// <item>
/// <term>SearchByExtensionVisitor</term>
/// <description>依副檔名搜尋檔案。</description>
/// </item>
/// </list>
/// </remarks>
```

表格形式需要 `<listheader>` 定義欄位標題：

```csharp
/// <remarks>
/// <list type="table">
/// <listheader>
/// <term>檔案類型</term>
/// <description>副檔名</description>
/// </listheader>
/// <item>
/// <term>Word 文件</term>
/// <description>.docx</description>
/// </item>
/// <item>
/// <term>純文字</term>
/// <description>.txt</description>
/// </item>
/// </list>
/// </remarks>
```

### 文字格式化

```xml
<b>粗體</b>
<i>斜體</i>
<u>底線</u>
```

編譯器和 Visual Studio 都會驗證這些 HTML 格式標籤。格式化的文字會出現在 IntelliSense 和產生的文件中。

---

## 連結與參照標籤（Link Tags）

### `<see>` — 行內連結

在文字中建立連結，有三種用法：

```xml
<!-- 連結到程式元素 -->
<see cref="Directory"/>
<see cref="Directory.Add(FileSystemComponent)">Add 方法</see>

<!-- 連結到外部 URL -->
<see href="https://example.com">連結文字</see>

<!-- 參照語言關鍵字 -->
<see langword="null"/>
<see langword="true"/>
<see langword="async"/>
```

`cref` 用於程式碼參照 — 編譯器會驗證目標是否存在，文件工具自動產生超連結。
`href` 用於外部 URL — 會建立可點擊的超連結。不要用 `cref` 來連結外部網址，因為 `cref` 不會產生可點擊的連結。

```csharp
/// <summary>
/// 實作 <see cref="IFileSystemVisitor"/> 介面，
/// 將檔案系統結構輸出為 XML 格式。
/// 格式規範請參考
/// <see href="https://example.com/spec">輸出格式規格</see>。
/// </summary>
public class XmlExportVisitor : IFileSystemVisitor
```

### `<seealso>` — 另請參閱

產生「See Also」區段的連結。不能巢狀在 `<summary>` 內。

```csharp
/// <summary>
/// 以樹狀結構顯示檔案系統。
/// </summary>
/// <seealso cref="XmlExportVisitor"/>
/// <seealso href="https://en.wikipedia.org/wiki/Visitor_pattern">Visitor Pattern</seealso>
public class DisplayVisitor : IFileSystemVisitor
```

### `cref` 屬性語法

`cref` 可以參照型別、方法、屬性等程式元素。泛型型別使用大括號：

```csharp
/// <see cref="List{T}"/>
/// <see cref="IDictionary{TKey, TValue}"/>
/// <see cref="Directory.Add(FileSystemComponent)"/>
```

---

## 文件重用標籤（Reuse Tags）

### `<inheritdoc>` — 繼承文件

從基底類別、介面或指定成員繼承 XML 註解，避免複製貼上。特別適合介面實作和方法覆寫。

```xml
<inheritdoc/>
<inheritdoc cref="member"/>
<inheritdoc path="xpath"/>
```

```csharp
public interface IFileSystemVisitor
{
    /// <summary>
    /// 走訪指定的目錄節點。
    /// </summary>
    /// <param name="directory">要走訪的目錄。</param>
    void Visit(Directory directory);
}

public class DisplayVisitor : IFileSystemVisitor
{
    /// <inheritdoc/>
    public void Visit(Directory directory)
    {
        // Visual Studio IDE 會自動顯示繼承的文件
        // 但對外發佈的程式庫應明確使用 <inheritdoc> 標籤，
        // 因為自動繼承不會寫入編譯器產生的 XML 文件檔
    }
}
```

`cref` 屬性可從非繼承關係的成員複製文件：

```csharp
/// <inheritdoc cref="ProcessAsync(string)"/>
public Result Process(string input)
```

`path` 屬性使用 XPath 篩選要繼承的部分：

```csharp
/// <inheritdoc path="/summary"/>
```

### `<include>` — 從外部檔案載入

從外部 XML 檔案載入文件內容。適合文件與程式碼分離管理的場景。

```xml
<include file='filename' path='tagpath[@name="id"]' />
```

```csharp
/// <include file='docs/api.xml' path='docs/member[@name="Directory.Add"]/*' />
public void Add(FileSystemComponent component)
```

---

## 泛型標籤（Generic Tags）

### `<typeparam>` — 描述型別參數

描述泛型型別或方法的型別參數。文字會顯示在 IntelliSense 中。

```xml
<typeparam name="TResult">description</typeparam>
```

```csharp
/// <summary>
/// 走訪檔案系統樹並收集結果。
/// </summary>
/// <typeparam name="TResult">走訪結果的型別。</typeparam>
public abstract class CollectorVisitor<TResult> : IFileSystemVisitor
```

### `<typeparamref>` — 在文字中參照型別參數

在文字中提及型別參數時使用，文件工具會以斜體等方式區別顯示。

```csharp
/// <summary>
/// 回傳收集到的 <typeparamref name="TResult"/> 結果。
/// </summary>
```

---

## 完整範例

以下示範如何為一個類別及其成員撰寫完整的文件註解：

```csharp
/// <summary>
/// 以遞迴方式走訪檔案系統樹，搜尋符合指定副檔名的檔案。
/// </summary>
/// <remarks>
/// <para>
/// 此 Visitor 實作 <see cref="IFileSystemVisitor"/> 介面，
/// 使用 Visitor Pattern 走訪 Composite 結構。
/// </para>
/// <para>
/// 搜尋結果可透過 <see cref="Results"/> 屬性取得。
/// 副檔名比對不區分大小寫。
/// </para>
/// </remarks>
/// <example>
/// 搜尋所有 Word 文件：
/// <code>
/// var visitor = new SearchByExtensionVisitor(".docx");
/// root.Accept(visitor);
/// var results = visitor.Results; // List&lt;File&gt;
/// </code>
/// </example>
/// <seealso cref="DisplayVisitor"/>
/// <seealso cref="SizeCalculatorVisitor"/>
public class SearchByExtensionVisitor : IFileSystemVisitor
{
    /// <summary>
    /// 初始化 <see cref="SearchByExtensionVisitor"/> 的新執行個體。
    /// </summary>
    /// <param name="extension">
    /// 要搜尋的副檔名，包含前導點號（例如 ".docx"）。
    /// 比對不區分大小寫。
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="extension"/> 為 <see langword="null"/> 時拋出。
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="extension"/> 不以點號開頭時拋出。
    /// </exception>
    public SearchByExtensionVisitor(string extension) { }

    /// <summary>
    /// 取得搜尋結果中所有符合的檔案。
    /// </summary>
    /// <value>
    /// 副檔名符合的 <see cref="File"/> 集合。
    /// 若未找到任何符合的檔案，回傳空集合。
    /// </value>
    public IReadOnlyList<File> Results { get; }

    /// <inheritdoc/>
    public void Visit(Directory directory) { }

    /// <inheritdoc/>
    public void Visit(WordDocument file) { }

    /// <inheritdoc/>
    public void Visit(ImageFile file) { }

    /// <inheritdoc/>
    public void Visit(TextFile file) { }
}
```

---

## 撰寫原則與常見反模式

### 好的文件註解

- **寫「為什麼」和「什麼」，而非「怎麼做」** — 使用者想知道這個 API 的用途和行為，而非內部實作細節
- **描述契約** — 說明前置條件、後置條件、參數限制、回傳值意義
- **用 `<see langword="..."/>` 標記關鍵字** — 如 `null`、`true`、`false`、`async`，而非用純文字或 `<c>null</c>`
- **善用 `<inheritdoc/>`** — 介面已有完整文件時，實作類別不必重複撰寫
- **副檔名、格式等使用 `<c>` 標記** — 如 `<c>.docx</c>`，讓它以程式碼字型顯示

### 常見反模式

```csharp
// 無意義的重複 — summary 只是把方法名稱用自然語言重述
/// <summary>
/// Gets the name.
/// </summary>
public string Name { get; }

// 應改為描述 Name 代表什麼
/// <summary>
/// 取得此檔案系統元件的顯示名稱，不包含路徑。
/// </summary>
public string Name { get; }
```

```csharp
// 洩漏實作細節 — 使用者不需要知道你用什麼演算法
/// <summary>
/// 用 DFS 走訪所有子節點，用 StringBuilder 組合 XML 字串。
/// </summary>

// 應改為描述行為和用途
/// <summary>
/// 將此目錄及其所有內容匯出為 XML 格式。
/// </summary>
```

```csharp
// 遺漏 <param> — 編譯器會發出警告
/// <summary>
/// 加入元件。
/// </summary>
public void Add(FileSystemComponent component)

// 應為每個參數補上 <param>
/// <summary>
/// 將指定的元件加入此目錄。
/// </summary>
/// <param name="component">要加入的檔案或子目錄。</param>
public void Add(FileSystemComponent component)
```

### 角括號的跳脫

在文件註解中使用 `<` 和 `>` 時，必須以 HTML 實體跳脫：

```csharp
/// <summary>
/// 此屬性的值永遠 &lt; 1。
/// </summary>
```

或在範例程式碼中使用泛型時：

```csharp
/// <example>
/// <code>
/// var list = new List&lt;string&gt;();
/// </code>
/// </example>
```

---

## 標籤速查表

| 類別 | 標籤 | 用途 | IntelliSense |
|------|------|------|:---:|
| 通用 | `<summary>` | 型別/成員簡述 | Yes |
| 通用 | `<remarks>` | 補充說明 | — |
| 成員 | `<param>` | 描述參數 | Yes |
| 成員 | `<paramref>` | 文字中參照參數 | — |
| 成員 | `<returns>` | 描述回傳值 | Yes |
| 成員 | `<exception>` | 描述例外 | — |
| 成員 | `<value>` | 描述屬性值 | Yes |
| 格式 | `<para>` | 段落（雙倍行距） | — |
| 格式 | `<br/>` | 換行（單倍行距） | — |
| 格式 | `<c>` | 行內程式碼 | — |
| 格式 | `<code>` | 多行程式碼區塊 | — |
| 格式 | `<example>` | 使用範例 | — |
| 格式 | `<list>` | 列表/表格 | — |
| 格式 | `<b>` `<i>` `<u>` | 粗體/斜體/底線 | — |
| 連結 | `<see cref="..."/>` | 行內程式碼參照 | — |
| 連結 | `<see href="..."/>` | 行內外部連結 | — |
| 連結 | `<see langword="..."/>` | 語言關鍵字參照 | — |
| 連結 | `<seealso>` | 另請參閱區段 | — |
| 重用 | `<inheritdoc/>` | 繼承基底/介面文件 | — |
| 重用 | `<include>` | 從外部 XML 載入 | — |
| 泛型 | `<typeparam>` | 描述型別參數 | Yes |
| 泛型 | `<typeparamref>` | 文字中參照型別參數 | — |

---

## 參考資料

- [Microsoft - Recommended XML tags for C# documentation comments](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags)
- [Microsoft - XML documentation comments (C# language reference)](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/)
