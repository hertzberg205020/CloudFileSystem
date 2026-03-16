# CloudFileSystem — OOD Class Diagram

套用 Composite Pattern + Visitor Pattern 後的 UML 類別圖。

## 完整類別圖

```mermaid
classDiagram
    direction TB

    %% ===== Composite Pattern =====
    class FileSystemComponent {
        <<abstract>>
        #string Name
        +FileSystemComponent? Parent
        +GetSize()* long
        +Accept(IFileSystemVisitor visitor)* void
        +GetPath() string
    }

    class Directory {
        -List~FileSystemComponent~ _children
        +IReadOnlyList~FileSystemComponent~ Children
        +Directory(string name)
        +Add(FileSystemComponent component) void
        +Remove(FileSystemComponent component) void
        +GetSize() long
        +Accept(IFileSystemVisitor visitor) void
    }

    class File {
        <<abstract>>
        +long Size
        +DateTime CreatedAt
        #File(string name, long size, DateTime createdAt)
        +GetSize() long
    }

    class WordDocument {
        +int PageCount
        +WordDocument(string name, long size, DateTime createdAt, int pageCount)
        +Accept(IFileSystemVisitor visitor) void
    }

    class ImageFile {
        +int Width
        +int Height
        +ImageFile(string name, long size, DateTime createdAt, int width, int height)
        +Accept(IFileSystemVisitor visitor) void
    }

    class TextFile {
        +string Encoding
        +TextFile(string name, long size, DateTime createdAt, string encoding)
        +Accept(IFileSystemVisitor visitor) void
    }

    %% ===== Visitor Pattern =====
    class IFileSystemVisitor {
        <<interface>>
        +Visit(Directory directory) void
        +Visit(WordDocument file) void
        +Visit(ImageFile file) void
        +Visit(TextFile file) void
    }

    class DisplayVisitor {
        -int _indentLevel
        +Visit(Directory directory) void
        +Visit(WordDocument file) void
        +Visit(ImageFile file) void
        +Visit(TextFile file) void
    }

    class SizeCalculatorVisitor {
        +long TotalSize
        +Visit(Directory directory) void
        +Visit(WordDocument file) void
        +Visit(ImageFile file) void
        +Visit(TextFile file) void
    }

    class SearchByExtensionVisitor {
        -string _targetExtension
        +List~string~ Results
        +SearchByExtensionVisitor(string extension)
        +Visit(Directory directory) void
        +Visit(WordDocument file) void
        +Visit(ImageFile file) void
        +Visit(TextFile file) void
    }

    class XmlExportVisitor {
        -StringBuilder _xmlBuilder
        -int _indentLevel
        +string GetXml()
        +Visit(Directory directory) void
        +Visit(WordDocument file) void
        +Visit(ImageFile file) void
        +Visit(TextFile file) void
    }

    %% ===== CLI =====
    class CloudFileSystemCli {
        -Directory _root
        +CloudFileSystemCli(Directory root)
        +Start() void
        -ExecuteCommand(int command) void
    }

    %% ===== Inheritance (Composite) =====
    FileSystemComponent <|-- Directory
    FileSystemComponent <|-- File
    File <|-- WordDocument
    File <|-- ImageFile
    File <|-- TextFile

    %% ===== Composition: Directory holds children =====
    Directory "1" *-- "0..*" FileSystemComponent : children

    %% ===== Visitor implements =====
    IFileSystemVisitor <|.. DisplayVisitor
    IFileSystemVisitor <|.. SizeCalculatorVisitor
    IFileSystemVisitor <|.. SearchByExtensionVisitor
    IFileSystemVisitor <|.. XmlExportVisitor

    %% ===== Dependencies =====
    FileSystemComponent ..> IFileSystemVisitor : Accept(visitor)
    CloudFileSystemCli --> Directory : _root
```

## 設計模式角色對照

### Composite Pattern

| GoF 角色 | 本專案類別 | 說明 |
|----------|-----------|------|
| **Component** | `FileSystemComponent` | 抽象基底類別，定義 `GetSize()` 與 `Accept()` 統一介面 |
| **Composite** | `Directory` | 持有 `List<FileSystemComponent>` children，`GetSize()` 遞迴加總 |
| **Leaf** | `File` (abstract) → `WordDocument`, `ImageFile`, `TextFile` | `GetSize()` 回傳自身 `Size` |

### Visitor Pattern

| GoF 角色 | 本專案類別 | 說明 |
|----------|-----------|------|
| **Visitor** | `IFileSystemVisitor` | 定義 `Visit()` overloads，每種 concrete element 一個 |
| **ConcreteVisitor** | `DisplayVisitor`, `SizeCalculatorVisitor`, `SearchByExtensionVisitor`, `XmlExportVisitor` | 各自實作一種遍歷操作 |
| **Element** | `FileSystemComponent` | 定義 `Accept(IFileSystemVisitor)` |
| **ConcreteElement** | `Directory`, `WordDocument`, `ImageFile`, `TextFile` | 實作 `Accept()` → 呼叫 `visitor.Visit(this)` |

## Accept / Visit 互動流程

```mermaid
sequenceDiagram
    participant CLI as CloudFileSystemCli
    participant Root as Directory (Root)
    participant Visitor as IFileSystemVisitor
    participant Child as FileSystemComponent

    CLI->>Visitor: new ConcreteVisitor()
    CLI->>Root: Accept(visitor)
    Root->>Visitor: visitor.Visit(this)
    Note over Visitor: 處理 Directory 邏輯<br/>(印出、計算、搜尋...)
    loop 遍歷 Children
        Root->>Child: child.Accept(visitor)
        Child->>Visitor: visitor.Visit(this)
        Note over Visitor: 根據 concrete type<br/>dispatch 到對應 overload
    end
```

## Traverse Log 整合方式

功能三要求在執行操作時印出走訪節點順序。每個 Visitor 的 `Visit()` 方法開頭加入：

```csharp
Console.WriteLine($"Visiting: {directory.GetPath()}");
// 或
Console.WriteLine($"Visiting: {file.GetPath()}");
```

這樣每次遍歷都會自動產生類似以下的 log：

```
Visiting: Root
Visiting: Root/Project_Docs
Visiting: Root/Project_Docs/需求規格書.docx
Visiting: Root/Project_Docs/系統架構圖.png
...
```
