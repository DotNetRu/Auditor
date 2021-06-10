using System.IO;

namespace DotNetRu.Auditor.Storage.FileSystem
{
    public interface IFile : IFileSystemEntry
    {
        Stream OpenForRead();

        Stream OpenForWrite();
    }
}