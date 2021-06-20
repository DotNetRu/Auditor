using System.IO;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.Storage.FileSystem
{
    public interface IWritableFile : IFile
    {
        ValueTask<Stream> OpenForWriteAsync();

        ValueTask<bool> DeleteAsync();
    }
}
