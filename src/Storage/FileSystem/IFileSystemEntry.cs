using System.Threading.Tasks;

namespace DotNetRu.Auditor.Storage.FileSystem
{
    public interface IFileSystemEntry
    {
        string Name { get; }

        string FullName { get; }

        ValueTask<bool> ExistsAsync();
    }
}
