using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.Storage.FileSystem
{
    public interface IFile : IFileSystemEntry
    {
        ValueTask<Stream> OpenForReadAsync();

        ValueTask<bool> RequestWriteAccessAsync([NotNullWhen(true)] out IWritableFile? writableFile);
    }
}
