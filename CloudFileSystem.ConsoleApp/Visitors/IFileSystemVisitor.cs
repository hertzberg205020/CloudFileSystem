using CloudFileSystem.ConsoleApp.Models;

namespace CloudFileSystem.ConsoleApp.Visitors;

public interface IFileSystemVisitor
{
    void Visit(Directory directory);
    void Visit(WordDocument file);
    void Visit(ImageFile file);
    void Visit(TextFile file);
}
