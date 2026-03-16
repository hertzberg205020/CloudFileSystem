# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test Commands

```bash
dotnet build                          # Build the solution
dotnet run --project CloudFileSystem.ConsoleApp  # Run the console app
dotnet test                           # Run all tests
dotnet test --filter "FullyQualifiedName~ClassName.MethodName"  # Run a single test
```

## Architecture

Cloud File System console app demonstrating four design patterns in C# (.NET 10):

### Design Patterns

- **Composite Pattern**: `FileSystemComponent` (abstract) → `Directory` (composite) and `File` (abstract leaf) → `WordDocument`, `ImageFile`, `TextFile`. Directory holds `List<FileSystemComponent>` for uniform tree traversal.
- **Visitor Pattern**: `IFileSystemVisitor` with `Visit()` overloads per component type. Concrete visitors: `DisplayVisitor`, `SizeCalculatorVisitor`, `SearchByExtensionVisitor`, `XmlExportVisitor`.
- **Command Pattern**: `ICommand` (`Execute()`, `Undo()`) for all mutation operations. `CommandManager` uses two `Stack<ICommand>` for undo/redo history. Concrete commands: `DeleteCommand`, `PasteCommand`, `SortCommand`, `TagCommand`, `UntagCommand`.
- **Prototype Pattern**: `FileSystemComponent.DeepCopy()` for recursive deep copy of composite tree structures. Used by `PasteCommand` to clone clipboard contents while maintaining correct `Parent` references via `Directory.Add()`.
- **I/O Abstraction**: `IConsole` interface with `SystemConsole` production impl. `CloudFileSystemCli` takes `IConsole` via constructor injection for testability.

### Key Design Decisions

- All mutation operations (delete, paste, sort, tag, untag) go through `CommandManager` — read-only operations (display, size, search, xml) execute directly
- `copy` stores a reference in CLI's `_clipboard`; deep copy happens at `paste` time
- Tags (`Urgent`, `Work`, `Personal`) live on `FileSystemComponent` base class (all components can be tagged)
- Sort persistently modifies `Directory._children` order (undoable via `SortCommand`)
- Paste auto-renames on name conflict (e.g., `file (1).txt`)

### Project Layout

- `CloudFileSystem.ConsoleApp/Models/` — Composite layer + enums (`Tag`, `SortBy`, `SortOrder`)
- `CloudFileSystem.ConsoleApp/Visitors/` — Visitor layer (IFileSystemVisitor + 4 implementations)
- `CloudFileSystem.ConsoleApp/Commands/` — Command layer (ICommand + CommandManager + 5 concrete commands)
- `CloudFileSystem.ConsoleApp/CloudFileSystemCli.cs` — Interactive CLI with command loop, holds `CommandManager` and `_clipboard`
- `CloudFileSystem.Tests/` — xUnit test project
- `test-cases/` — Golden files (`plain-text.out`, `xml-format.out`) for output comparison tests
- `spec/class-diagram.md` — Mermaid UML class diagram (all 4 patterns)
- `docs/requirements.md` — Full requirements spec
- `.reference/` — Java reference implementations (Composite, Command, Chain of Responsibility patterns)

### Testing Strategy

- **IConsole stub**: `TestConsole` injects fake input queue and captures output buffer for CLI integration tests
- **Golden file comparison**: Build sample structure via `SampleStructureFactory` → run Visitor → compare output against `test-cases/*.out`
- **Frameworks**: xUnit + NSubstitute + FluentAssertions
- **Naming convention**: `MethodName_StateUnderTest_ExpectedBehavior`

## Conventions

- **Commit messages**: Conventional Commits format (`feat:`, `fix:`, `docs:`, etc.) with Chinese descriptions
- **Language**: Code in English (C# naming), documentation and commit messages in Chinese (zh-TW)
- **Formatter**: CSharpier for C# code formatting
- Refer to `.claude/skills/` for detailed guides on project architecture, TDD workflow, and xUnit best practices
