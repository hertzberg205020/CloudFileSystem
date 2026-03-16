---
name: xunit-best-practices
description: "xUnit 測試框架最佳實務與完整 API 指南。涵蓋 Fact/Theory、資料驅動測試（InlineData/MemberData/ClassData/TheoryData）、斷言 API 完整用法、共享上下文（IClassFixture/ICollectionFixture/AssemblyFixture）、測試生命週期、平行執行、ITestOutputHelper、非同步測試、xUnit Analyzer 規則與設定檔。當使用者詢問 xUnit 的用法、想了解某個 xUnit 功能怎麼用、遇到 xUnit 相關錯誤、需要寫資料驅動測試、需要共享測試資源、想設定平行執行、或遇到 xUnit analyzer 警告時，都應觸發此 skill。即使使用者只是說「怎麼用 Theory」、「xUnit 怎麼共享資料庫連線」、「測試怎麼印 log」，也應觸發。"
---

# xUnit 測試框架最佳實務指南

本指南聚焦於 xUnit 框架特有的功能與最佳用法。TDD 工作流程、AAA 模式、命名慣例等通用測試原則請參考 `csharp-tdd` skill。

---

## [Fact] vs [Theory]

`[Fact]` 用於不需要參數的測試 — 每次執行條件固定，結果確定。
`[Theory]` 用於參數化測試 — 同一邏輯，多組輸入，每組資料作為獨立測試案例執行。

```csharp
// Fact — 固定輸入、固定預期
[Fact]
public void Count_EmptyStack_ReturnsZero()
{
    var stack = new Stack<int>();
    Assert.Equal(0, stack.Count);
}

// Theory — 同一邏輯，多組輸入
[Theory]
[InlineData(2, true)]
[InlineData(3, false)]
[InlineData(0, true)]
public void IsEven_VariousInputs_ReturnsExpected(int value, bool expected)
{
    Assert.Equal(expected, value % 2 == 0);
}
```

選擇原則：如果你發現自己在 `[Fact]` 裡複製貼上只改資料，就該用 `[Theory]`。

---

## 資料驅動測試

xUnit 提供三種方式為 `[Theory]` 提供資料，依複雜度遞增：

### InlineData — 簡單值型別

適合少量、簡單的測試資料。每個 `[InlineData]` 對應一次測試執行。

```csharp
[Theory]
[InlineData("test.docx", ".docx", true)]
[InlineData("image.png", ".docx", false)]
[InlineData("notes.txt", ".txt", true)]
public void HasExtension_ReturnsCorrectResult(
    string fileName, string extension, bool expected)
{
    var result = Path.GetExtension(fileName) == extension;
    Assert.Equal(expected, result);
}
```

限制：只能傳編譯期常數（string、int、bool、enum 等），不能傳物件。

### MemberData — 共用或動態資料

當資料需要計算、含複雜物件、或多個測試共用同一組資料時使用。指向同類別的靜態屬性或方法。

```csharp
// 使用靜態屬性
public static TheoryData<int, string> PropertyData => new()
{
    { 1, "Hello" },
    { 2, "World" }
};

[Theory]
[MemberData(nameof(PropertyData))]
public void Process_VariousInputs_Succeeds(int id, string name)
{
    Assert.NotNull(name);
}

// 使用靜態方法（可帶參數）
public static IEnumerable<object[]> GetTestCases()
{
    yield return new object[] { new List<int> { 1, 2, 3 }, 6 };
    yield return new object[] { new List<int>(), 0 };
}

[Theory]
[MemberData(nameof(GetTestCases))]
public void Sum_VariousLists_ReturnsExpected(List<int> input, int expected)
{
    Assert.Equal(expected, input.Sum());
}
```

**推薦使用 `TheoryData<T>`**：比 `IEnumerable<object[]>` 更好，因為它提供編譯期型別檢查，參數數量或型別不對會在編譯時就報錯，而非執行時。

### ClassData — 大量或跨類別共用資料

當資料邏輯複雜到值得獨立成類別，或需要跨多個測試類別共用時使用。

```csharp
// v2: 實作 IEnumerable<object[]>
public class FileTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { "report.pdf", 1024L };
        yield return new object[] { "photo.jpg", 2048L };
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

// v3: 實作 IEnumerable<TheoryDataRow<T>>（型別安全）
public class FileTestDataV3 : IEnumerable<TheoryDataRow<string, long>>
{
    public IEnumerator<TheoryDataRow<string, long>> GetEnumerator()
    {
        yield return new("report.pdf", 1024L);
        yield return new("photo.jpg", 2048L);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

[Theory]
[ClassData(typeof(FileTestData))]
public void ProcessFile_VariousFiles_Succeeds(string name, long size)
{
    Assert.True(size > 0);
}
```

### 資料穩定性注意事項

避免在 `MemberData` 中使用動態值（如 `DateTime.Now`、`Guid.NewGuid()`）。Visual Studio Test Explorer 在探索階段會執行一次資料產生器來列舉測試案例，執行階段再呼叫一次 — 若兩次產生的值不同，測試會被標記為「未發現」或「已忽略」。

---

## 斷言 API

xUnit 的斷言以靜態方法形式提供，涵蓋值比較、集合、例外、型別等場景。

### 值比較

```csharp
Assert.Equal(expected, actual);                  // 值相等（使用 .Equals()）
Assert.NotEqual(expected, actual);               // 值不相等
Assert.Equal(3.14, actual, precision: 2);        // 浮點數精度比較
Assert.Equal("hello", actual, ignoreCase: true); // 字串忽略大小寫
Assert.Equal(expected, actual, comparer);        // 自訂 IEqualityComparer<T>
Assert.Equal(expected, actual, (a, b) => ...);   // 自訂比較函式（lambda）
```

### 布林與 Null

```csharp
Assert.True(condition);
Assert.False(condition);
Assert.Null(obj);
Assert.NotNull(obj);
```

### 集合

```csharp
Assert.Empty(collection);
Assert.NotEmpty(collection);
Assert.Single(collection);                       // 恰好一個元素
Assert.Contains(item, collection);
Assert.DoesNotContain(item, collection);
Assert.Contains(collection, x => x.Name == "test"); // predicate 版本
Assert.All(collection, item => Assert.True(item > 0)); // 每個元素都符合

// 集合相等 — 支援跨型別比較
Assert.Equal(new[] { 1, 2, 3 }, new List<int> { 1, 2, 3 });
```

### 字串

```csharp
Assert.Contains("sub", fullString);
Assert.DoesNotContain("sub", fullString);
Assert.StartsWith("prefix", fullString);
Assert.EndsWith("suffix", fullString);
Assert.Matches(@"\d{3}-\d{4}", fullString);      // 正規表達式
```

### 例外

```csharp
// 驗證拋出特定例外
var ex = Assert.Throws<ArgumentNullException>(() => service.Process(null));
Assert.Equal("input", ex.ParamName);  // 可進一步驗證例外屬性

// 非同步版本
var ex = await Assert.ThrowsAsync<InvalidOperationException>(
    () => service.ProcessAsync(badInput));
```

### 型別

```csharp
Assert.IsType<FileNode>(node);                   // 精確型別
Assert.IsNotType<DirectoryNode>(node);
Assert.IsAssignableFrom<INode>(node);            // 可指派（含繼承與介面）
```

### 範圍

```csharp
Assert.InRange(value, low: 1, high: 100);
Assert.NotInRange(value, low: 50, high: 60);
```

### 事件

```csharp
var evt = Assert.Raises<EventArgs>(
    h => obj.MyEvent += h,
    h => obj.MyEvent -= h,
    () => obj.TriggerEvent());
Assert.NotNull(evt);
```

### 多重斷言（xUnit v3）

xUnit v3 新增 `Assert.Multiple`，允許收集所有失敗而非在第一個失敗就停止：

```csharp
Assert.Multiple(
    () => Assert.Equal("expected1", actual1),
    () => Assert.Equal("expected2", actual2),
    () => Assert.True(condition)
);
```

---

## 測試生命週期

xUnit 為每個測試方法建立新的測試類別實例 — 這是刻意的設計，確保測試之間完全隔離，不會有共用可變狀態導致的順序依賴問題。

### 建構子 = Setup，IDisposable = Teardown

```csharp
public class StackTests : IDisposable
{
    private readonly Stack<int> _stack;

    public StackTests()
    {
        // 每個測試方法執行前呼叫（等同其他框架的 Setup）
        _stack = new Stack<int>();
    }

    public void Dispose()
    {
        // 每個測試方法執行後呼叫（等同 Teardown）
        // 用於釋放非託管資源
    }

    [Fact]
    public void Push_AddsItem()
    {
        _stack.Push(42);
        Assert.Single(_stack);
    }
}
```

### IAsyncLifetime — 非同步 Setup/Teardown

當初始化或清理需要 `await` 時使用：

```csharp
public class DatabaseTests : IAsyncLifetime
{
    private DbConnection _connection;

    public async Task InitializeAsync()
    {
        _connection = new SqlConnection("...");
        await _connection.OpenAsync();
    }

    public async Task DisposeAsync()
    {
        await _connection.CloseAsync();
    }

    [Fact]
    public async Task Query_ReturnsData()
    {
        var result = await _connection.QueryAsync("SELECT 1");
        Assert.NotEmpty(result);
    }
}
```

---

## 共享上下文（Shared Context）

當多個測試需要共用昂貴的資源（資料庫連線、HTTP client、測試容器）時，每個測試都重建一次太慢。xUnit 提供三個層級的共享機制：

### IClassFixture — 同一測試類別內共用

Fixture 在該類別的第一個測試前建立，最後一個測試後銷毀。同一類別內所有測試共用同一個 Fixture 實例。

```csharp
public class DatabaseFixture : IDisposable
{
    public SqlConnection Db { get; private set; }

    public DatabaseFixture()
    {
        Db = new SqlConnection("MyConnectionString");
        // 初始化測試資料
    }

    public void Dispose()
    {
        // 清理測試資料
    }
}

public class MyDatabaseTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public MyDatabaseTests(DatabaseFixture fixture)
    {
        _fixture = fixture;  // 透過建構子注入
    }

    [Fact]
    public void Query_ReturnsExpectedData()
    {
        // 使用 _fixture.Db
    }
}
```

### ICollectionFixture — 跨測試類別共用

當多個測試類別需要共用同一個 Fixture 時，定義一個 Collection。Collection 內的所有類別共用一個 Fixture 實例，且預設不平行執行（因為它們共用狀態）。

```csharp
// 1. 定義 Collection（空殼類別，僅作為標記）
[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // 這個類別不需要任何程式碼
}

// 2. 標記測試類別屬於此 Collection
[Collection("Database collection")]
public class UserRepositoryTests
{
    private readonly DatabaseFixture _fixture;
    public UserRepositoryTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public void GetUser_ExistingId_ReturnsUser() { /* 使用 _fixture.Db */ }
}

[Collection("Database collection")]
public class OrderRepositoryTests
{
    private readonly DatabaseFixture _fixture;
    public OrderRepositoryTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public void GetOrders_ValidUser_ReturnsOrders() { /* 使用 _fixture.Db */ }
}
```

### AssemblyFixture — 整個測試組件共用（xUnit v3）

最大範圍的共享，整個組件只建立一次。適合非常昂貴的初始化（如啟動 Docker 容器）。

```csharp
[assembly: AssemblyFixture(typeof(DatabaseFixture))]

// 任何測試類別都可以透過建構子注入
public class AnyTestClass
{
    private readonly DatabaseFixture _fixture;
    public AnyTestClass(DatabaseFixture fixture) => _fixture = fixture;
}
```

### 選擇指南

| 範圍 | 機制 | 適用場景 |
|------|------|----------|
| 每個測試方法 | 建構子 / IDisposable | 輕量物件，毫秒內可建立 |
| 同一測試類別 | IClassFixture\<T\> | 中等成本，如記憶體內資料庫 |
| 跨測試類別 | ICollectionFixture\<T\> | 昂貴資源，多個類別需要共用 |
| 整個組件 | AssemblyFixture（v3） | 非常昂貴，如 Docker 容器 |

---

## ITestOutputHelper — 測試輸出

xUnit 不使用 `Console.WriteLine`（因為平行執行時 stdout 會混在一起）。改用 `ITestOutputHelper`，輸出會附加到該測試的結果中。

```csharp
public class MyTests
{
    private readonly ITestOutputHelper _output;

    public MyTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void DebugTest()
    {
        var result = Calculate();
        _output.WriteLine("Calculated result: {0}", result);
        Assert.Equal(42, result);
    }
}
```

顯示輸出：

```bash
# 在終端機看到測試輸出
dotnet test --logger "console;verbosity=normal"

# 或啟用即時輸出
dotnet test --showliveoutput
```

也可在 `testconfig.json` 中設定：

```json
{
  "xUnit": {
    "showLiveOutput": true
  }
}
```

---

## 平行執行

xUnit 預設以 **Test Collection** 為單位平行執行。同一 Collection 內的測試循序執行，不同 Collection 之間平行。每個沒有明確標記 `[Collection]` 的測試類別自成一個 Collection。

### 設定平行度

```csharp
// 設定最大平行執行緒數
[assembly: CollectionBehavior(MaxParallelThreads = 4)]

// 完全停用平行執行
[assembly: CollectionBehavior(DisableTestParallelization = true)]

// 所有測試放入同一個 Collection（循序執行）
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]
```

### 停用特定 Collection 的平行

```csharp
[CollectionDefinition("Sequential tests", DisableParallelization = true)]
public class SequentialCollection { }

[Collection("Sequential tests")]
public class SlowIntegrationTests
{
    // 這些測試不會與同一 Collection 內的其他測試平行
}
```

### testconfig.json 設定

```json
{
  "xUnit": {
    "parallelizeTestCollections": false,
    "maxParallelThreads": 4,
    "culture": "en-US"
  }
}
```

---

## 非同步測試

xUnit 原生支援 `async Task` 和 `async ValueTask` 測試方法。不要用 `.Result` 或 `.Wait()` 阻塞非同步呼叫 — 這可能造成死鎖。

```csharp
[Fact]
public async Task GetUserAsync_ValidId_ReturnsUser()
{
    // Arrange
    var service = new UserService();

    // Act
    var user = await service.GetUserAsync(1);

    // Assert
    Assert.NotNull(user);
    Assert.Equal("Alice", user.Name);
}

[Fact]
public async Task DeleteAsync_InvalidId_ThrowsNotFoundException()
{
    var service = new UserService();

    await Assert.ThrowsAsync<NotFoundException>(
        () => service.DeleteAsync(-1));
}
```

---

## 常見 xUnit Analyzer 規則

xUnit 提供 Roslyn analyzer，在編譯時就幫你抓出常見錯誤。以下是最重要的幾條：

| 規則 | 說明 | 修正方式 |
|------|------|----------|
| xUnit1005 | `[Theory]` 缺少測試資料 | 加上 `[InlineData]`、`[MemberData]` 或 `[ClassData]` |
| xUnit1037 | `[ClassData]` 的型別參數數量與方法參數不符 | 確保資料列元素數量 = 方法參數數量 |
| xUnit1038 | `TheoryData<T>` 泛型參數數量與方法參數不符 | 對齊泛型參數與方法參數 |
| xUnit1041 | 在 Collection Definition 以外的地方用 `IClassFixture` | 將 `IClassFixture` 放在 `[CollectionDefinition]` 類別上 |
| xUnit2000 | `Assert.Equal(true, ...)` | 改用 `Assert.True(...)` |
| xUnit2002 | `Assert.Equal(null, ...)` | 改用 `Assert.Null(...)` |
| xUnit2007 | `Assert.IsType` 傳入不正確的型別 | 確認型別正確 |
| xUnit2013 | `Assert.Equal(0, collection.Count)` | 改用 `Assert.Empty(collection)` |

遇到 analyzer 警告時，遵循修正建議 — 這些規則源自 xUnit 團隊多年累積的最佳實務。

---

## 設定檔

### testconfig.json（推薦）

放在測試專案根目錄，xUnit 原生設定格式：

```json
{
  "xUnit": {
    "culture": "en-US",
    "diagnosticMessages": true,
    "parallelizeTestCollections": true,
    "maxParallelThreads": 0,
    "showLiveOutput": false
  }
}
```

### .runsettings（Visual Studio / dotnet test）

```xml
<RunSettings>
  <xUnit>
    <ParallelizeTestCollections>true</ParallelizeTestCollections>
    <MaxParallelThreads>0</MaxParallelThreads>
    <ShowLiveOutput>true</ShowLiveOutput>
  </xUnit>
</RunSettings>
```

使用方式：`dotnet test --settings test.runsettings`

---

## 快速參考

```bash
# 執行所有測試
dotnet test

# 執行特定測試專案
dotnet test CloudFileSystem.Tests/

# 篩選特定測試
dotnet test --filter "FullyQualifiedName~DirectoryTests"
dotnet test --filter "Category=Integration"

# 顯示詳細輸出
dotnet test --logger "console;verbosity=normal"

# 收集程式碼覆蓋率
dotnet test --collect:"XPlat Code Coverage"
```
