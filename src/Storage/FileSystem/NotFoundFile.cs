using System;
using System.IO;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.Storage.FileSystem
{
    public class NotFoundFile : FileSystemEntry, IFile
    {
        public NotFoundFile(string fullName)
            : base(fullName, false)
        {
        }

        public static Task<IFile> ToTask(string fullName) => Task.FromResult<IFile>(new NotFoundFile(fullName));

        public static Exception ToException(string fullName) => new FileNotFoundException($"Could not find file: {fullName}", fullName);

        public Stream OpenForRead() => throw ToException(FullName);

        public Stream OpenForWrite() => throw ToException(FullName);
    }
}
