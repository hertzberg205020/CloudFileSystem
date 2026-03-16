using CloudFileSystem.ConsoleApp.Visitors;

namespace CloudFileSystem.ConsoleApp.Models;

/// <summary>
/// 檔案系統元件的抽象基底類別，為 Composite Pattern 中的 Component 角色。
/// </summary>
/// <remarks>
/// <para>所有檔案與目錄皆繼承此類別，透過 <see cref="Accept"/> 方法支援 Visitor Pattern 的雙重分派。</para>
/// <para>每個元件透過 <see cref="Parent"/> 維護父子關係，並可透過 <see cref="GetPath"/> 取得完整路徑。</para>
/// </remarks>
/// <seealso cref="Directory"/>
/// <seealso cref="File"/>
public abstract class FileSystemComponent
{
    /// <summary>
    /// 初始化 <see cref="FileSystemComponent"/> 的新執行個體。
    /// </summary>
    /// <param name="name">元件的顯示名稱。</param>
    protected FileSystemComponent(string name)
    {
        Name = name;
    }

    /// <summary>
    /// 取得此元件的顯示名稱，不包含路徑。
    /// </summary>
    /// <value>檔案或目錄的名稱，例如 <c>"需求規格書.docx"</c> 或 <c>"專案文件 (Project_Docs)"</c>。</value>
    public string Name { get; }

    /// <summary>
    /// 取得或設定此元件的父目錄。
    /// </summary>
    /// <value>父目錄的參考；若為根目錄則為 <see langword="null"/>。</value>
    public Directory? Parent { get; internal set; }

    /// <summary>
    /// 取得此元件的大小（單位：Bytes）。
    /// </summary>
    /// <returns>目錄回傳所有子元件的大小總和；檔案回傳自身大小。</returns>
    public abstract long GetSize();

    /// <summary>
    /// 接受 Visitor 走訪此元件，實現 Visitor Pattern 的雙重分派。
    /// </summary>
    /// <param name="visitor">要套用的 Visitor 實例。</param>
    public abstract void Accept(IFileSystemVisitor visitor);

    /// <summary>
    /// 取得此元件從根目錄到自身的完整路徑。
    /// </summary>
    /// <returns>以 <c>"/"</c> 分隔的完整路徑字串，例如 <c>"根目錄 (Root)/專案文件 (Project_Docs)/需求規格書.docx"</c>。</returns>
    public string GetPath()
    {
        if (Parent == null)
            return Name;
        return Parent.GetPath() + "/" + Name;
    }
}
