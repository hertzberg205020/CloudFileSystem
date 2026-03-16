# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test Commands

```bash
dotnet build                          # Build the solution
dotnet run --project CloudFileSystem.ConsoleApp  # Run the console app
dotnet test                           # Run all tests (once test project exists)
dotnet test --filter "FullyQualifiedName~ClassName.MethodName"  # Run a single test
```

## Architecture

This is a Cloud File System console app demonstrating **Composite + Visitor** design patterns in C# (.NET 10).

### Design Pattern Application

- **Composite Pattern**: `FileSystemComponent` (abstract) → `Directory` (composite, holds `List<FileSystemComponent>`) and `File` (abstract leaf) → `WordDocument`, `ImageFile`, `TextFile`. Enables uniform treatment of files and directories.
- **Visitor Pattern**: `IFileSystemVisitor` interface with `Visit()` overloads per component type. Concrete visitors: `DisplayVisitor`, `SizeCalculatorVisitor`, `SearchByExtensionVisitor`, `XmlExportVisitor`. Each visitor integrates Traverse Log output in its Visit methods.
- **I/O Abstraction**: `IConsole` interface (`ReadLine`, `Write`, `WriteLine`, `WriteError`) with `SystemConsole` production impl. `CloudFileSystemCli` takes `IConsole` via constructor injection for testability.

### Project Layout

- `CloudFileSystem.ConsoleApp/Models/` — Composite layer (FileSystemComponent, Directory, File subclasses)
- `CloudFileSystem.ConsoleApp/Visitors/` — Visitor layer (IFileSystemVisitor + 4 implementations)
- `CloudFileSystem.ConsoleApp/IConsole.cs`, `SystemConsole.cs` — I/O abstraction
- `CloudFileSystem.ConsoleApp/CloudFileSystemCli.cs` — Interactive CLI with command loop
- `CloudFileSystem.Tests/` — xUnit test project (planned)
- `test-cases/` — Golden files (`plain-text.out`, `xml-format.out`) for output comparison tests
- `spec/class-diagram.md` — Mermaid UML class diagram
- `docs/requirements.md` — Full requirements spec
- `.reference/` — Java reference implementations (Composite, Command, Chain of Responsibility patterns)

### Testing Strategy

- **IConsole stub**: Inject fake input queue and capture output buffer for CLI integration tests
- **Golden file comparison**: Build sample structure → run Visitor → compare output against `test-cases/*.out`
- **Frameworks**: xUnit + NSubstitute + FluentAssertions
- **Naming convention**: `MethodName_StateUnderTest_ExpectedBehavior`

## Conventions

- **Commit messages**: Conventional Commits format (`feat:`, `fix:`, `docs:`, etc.) with Chinese descriptions
- **Language**: Code in English (C# naming), documentation and commit messages in Chinese (zh-TW)
- **Formatter**: CSharpier for C# code formatting
- Refer to `.claude/skills/` for detailed guides on project architecture, TDD workflow, and xUnit best practices
