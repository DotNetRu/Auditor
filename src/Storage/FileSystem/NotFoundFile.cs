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

        public static ValueTask<IFile> ToTask(string fullName) => ValueTask.FromResult<IFile>(new NotFoundFile(fullName));

        public static Exception ToException(string fullName) => new FileNotFoundException($"Could not find file: {fullName}", fullName);

        public ValueTask<Stream> OpenForReadAsync() => throw ToException(FullName);

        public ValueTask<Stream> OpenForWriteAsync() => throw ToException(FullName);
    }
}
