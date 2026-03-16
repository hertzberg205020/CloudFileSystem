---
name: csharp-tdd
description: "C# TDD 最佳實務指南與工作流程。當使用者要求撰寫測試、進行測試驅動開發、建立測試專案、添加單元測試、或討論測試策略時，使用此 skill 引導開發流程。即使使用者只是說「幫我寫測試」、「加 unit test」、「用 TDD 做」、「測試這個功能」，都應觸發此 skill。"
---

# C# TDD 最佳實務指南

本指南規範在此專案中進行 TDD 開發的流程與原則。

## TDD 工作流程

當使用者要求實作功能或撰寫測試時，按以下流程執行：

1. **建立測試專案**（若尚未存在）
2. **寫一個會失敗的測試** — 定義最簡單的預期行為
3. **執行測試確認紅燈** (`dotnet test`)
4. **寫最少量的生產程式碼**使測試通過
5. **執行測試確認綠燈**
6. **重構** — 改善設計，確保測試仍通過
7. **重複** — 逐步增加測試案例，涵蓋邊界條件與異常情境

這就是 Red-Green-Refactor 循環。每次只處理一個小增量，因為小步驟更容易定位問題，也讓重構變得安全。重構是逐分鐘進行的持續活動，不是最後才做的事。

---

## 測試框架與工具

- **測試框架**：xUnit — 現代化設計，原生支援 DI 與 Theory-based 資料驅動測試
- **Mocking**：NSubstitute 或 Moq — NSubstitute 語法更簡潔直覺
- **斷言庫**：FluentAssertions — 讓斷言讀起來像自然語言，失敗訊息也更清楚
- **測試專案命名**：`{ProjectName}.Tests`
- **Target Framework**：與主專案一致（本專案為 net10.0）

### 測試專案 csproj 範本

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
    <PackageReference Include="FluentAssertions" Version="8.*" />
    <PackageReference Include="NSubstitute" Version="5.*" />
    <PackageReference Include="coverlet.collector" Version="6.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\{MainProject}\{MainProject}.csproj" />
  </ItemGroup>
</Project>
```

---

## 測試命名

採用 **MethodName_StateUnderTest_ExpectedBehavior** 模式。好的測試名稱等於可執行的文件 — 團隊成員不需讀測試內容就能從名稱理解被測行為，測試失敗時也能立即知道哪個場景出了問題。

```csharp
// 清楚的命名 — 從名稱就能理解測試目的
[Fact]
public void CalculateTotalSize_WithNestedDirectories_ReturnsSumOfAllFiles()

[Fact]
public void SearchByExtension_WithDocxExtension_ReturnsMatchingFiles()

// 模糊的命名 — 失敗時無法從名稱判斷問題
[Fact]
public void Test1()

[Fact]
public void TestCalculate()
```

### 測試檔案組織

按 SUT 的方法或行為分檔，避免一個巨大的測試類別：

```
Tests/
├── DirectoryTests/
│   ├── CalculateTotalSizeTests.cs
│   ├── SearchByExtensionTests.cs
│   └── ToXmlTests.cs
├── FileTests/
│   ├── WordFileTests.cs
│   └── ImageFileTests.cs
```

---

## Arrange-Act-Assert (AAA)

每個測試分三段。這個結構讓人一眼看出「準備了什麼、做了什麼、期望什麼」，降低測試本身出 bug 的機會。

```csharp
[Fact]
public void CalculateTotalSize_EmptyDirectory_ReturnsZero()
{
    // Arrange
    var directory = new Directory("EmptyDir");

    // Act
    var totalSize = directory.CalculateTotalSize();

    // Assert
    totalSize.Should().Be(0);
}
```

每個測試只有一個 Act。如果你想在一個測試裡做兩件事，這通常表示它該拆成兩個測試。多重 Act 的問題在於：第一個 Assert 失敗後，後面的 Assert 直接被跳過，你會漏掉重要資訊。

---

## 參數化測試

避免在測試中寫迴圈或條件邏輯（`if`、`for`、`while`），因為這讓測試本身變成需要除錯的程式碼。改用 xUnit 的 `[Theory]` + `[InlineData]` 來涵蓋多種輸入：

```csharp
[Theory]
[InlineData("test.docx", ".docx", true)]
[InlineData("image.png", ".docx", false)]
[InlineData("notes.txt", ".txt", true)]
public void HasExtension_VariousInputs_ReturnsCorrectResult(
    string fileName, string extension, bool expected)
{
    // Arrange
    var file = CreateTestFile(fileName);

    // Act
    var result = file.HasExtension(extension);

    // Assert
    result.Should().Be(expected);
}
```

資料較複雜時使用 `[MemberData]`：

```csharp
[Theory]
[MemberData(nameof(GetTestDirectories))]
public void CalculateTotalSize_VariousStructures_ReturnsExpectedSize(
    Directory directory, long expectedSize)
{
    var result = directory.CalculateTotalSize();
    result.Should().Be(expectedSize);
}

public static IEnumerable<object[]> GetTestDirectories()
{
    yield return new object[] { CreateEmptyDirectory(), 0L };
    yield return new object[] { CreateDirectoryWithOneFile(500), 500L };
}
```

---

## Mocking 原則

### 只 Mock 外部依賴

Mock 的目的是隔離不可控的外部因素（檔案系統、資料庫、網路、`DateTime.Now`），讓測試快速且可重複。對內部穩定的領域物件（如 `Directory` 和 `FileBase` 的協作）直接使用真實物件，因為過度 Mock 會破壞內聚性，讓測試與實作細節耦合，重構時反而成為阻力。

```csharp
// 用介面取代靜態依賴（Seam 模式）
public interface IDateTimeProvider
{
    DateTime Now { get; }
}

[Fact]
public void GetDiscountedPrice_OnTuesday_ReturnsHalfPrice()
{
    // Arrange
    var dateTimeProvider = Substitute.For<IDateTimeProvider>();
    dateTimeProvider.Now.Returns(new DateTime(2026, 3, 17)); // Tuesday
    var calculator = new PriceCalculator(dateTimeProvider);

    // Act
    var price = calculator.GetDiscountedPrice(100);

    // Assert
    price.Should().Be(50);
}
```

### 可測試性設計

- 依賴抽象（介面）而非具體實作 — 遵循依賴反轉原則
- 透過建構子注入依賴 — 測試時可輕鬆替換
- 避免在類別內部 `new` 外部依賴 — 這讓測試無法控制依賴行為

---

## 測試品質守則

### FIRST 原則

| 原則 | 說明 |
|------|------|
| **Fast** | 單元測試應在毫秒內完成。慢的測試讓開發者不想跑測試，TDD 循環就斷了。 |
| **Isolated** | 測試獨立運行，不依賴執行順序。共用可變狀態會讓測試變得不可預測。 |
| **Repeatable** | 同條件下永遠產生同結果。依賴外部狀態（如當前時間）的測試週二過、週三不過。 |
| **Self-Checking** | 自動判定通過或失敗，無需人工看 console 輸出。 |
| **Timely** | 寫測試的時間不應遠超寫產品程式碼的時間。如果很難測試，考慮改善設計。 |

### 避免的反模式

- **測試中的邏輯** — 測試裡出現 `if`/`for`/`while` 表示測試本身可能有 bug。拆成多個測試或用 `[Theory]`。
- **Magic String/Number** — 硬編碼值提取為具名常數，讓意圖更明確。
- **測試私有方法** — 透過公開方法間接驗證私有方法的行為。如果你覺得非得測試私有方法，通常表示該方法應該被提取為獨立的類別。
- **過度使用 Setup/Teardown** — 優先使用 Helper Method，因為每個測試可能需要不同的設定。在 xUnit 中使用建構子進行共用初始化。

---

## 測試輔助方法

用 Factory Method 減少 Arrange 區段的重複，同時保持每個測試的可讀性：

```csharp
public class DirectoryTests
{
    private static Directory CreateDirectoryWithFiles(
        params (string name, long sizeKb)[] files)
    {
        var dir = new Directory("TestDir");
        foreach (var (name, sizeKb) in files)
        {
            dir.Add(new TextFile(name, sizeKb, DateTime.Now, "UTF-8"));
        }
        return dir;
    }

    [Fact]
    public void CalculateTotalSize_MultipleFiles_ReturnsSumOfSizes()
    {
        // Arrange
        var dir = CreateDirectoryWithFiles(("file1.txt", 100), ("file2.txt", 200));

        // Act
        var total = dir.CalculateTotalSize();

        // Assert
        total.Should().Be(300);
    }
}
```

Helper Method 僅在同一測試類別內共用。不透過繼承基底測試類別共用 Arrange，因為這會造成脆弱的依賴鏈 — 基底類別一改，大量不相關的測試跟著壞。

---

## Aggregate 測試策略

對領域模型中的聚合根，以其公開介面進行測試，不單獨測試子實體。例如透過 `Directory` 的方法驗證 `FileBase` 的行為，而非直接測試 `FileBase`。這保持了聚合的封裝性 — 內部結構可以自由重構而不破壞測試。

---

## 程式碼覆蓋率

不追求 100% 覆蓋率 — 邊際效益遞減很快，追求最後 5% 的覆蓋率可能花費不成比例的時間。聚焦核心業務邏輯，建議目標 >= 80%。

收集覆蓋率：`dotnet test --collect:"XPlat Code Coverage"`

---

## 參考資料

- [Microsoft - Best practices for writing unit tests (.NET)](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
- [Ardalis - Mastering Unit Tests in .NET](https://ardalis.com/mastering-unit-tests-dotnet-best-practices-naming-conventions/)
- [Enterprise Craftsmanship - TDD Best Practices](https://enterprisecraftsmanship.com/posts/tdd-best-practices)
- [CodeJack - Best Practices for Unit Testing in .NET](https://codejack.com/2025/01/best-practices-for-unit-testing-in-net/)
