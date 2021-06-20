using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.Storage.FileSystem
{
    public class NotFoundFile : FileSystemEntry, IWritableFile
    {
        public NotFoundFile(string fullName)
            : base(fullName, false)
        {
        }

        public static ValueTask<IFile> AsTask(string fullName) => ValueTask.FromResult<IFile>(new NotFoundFile(fullName));

        public static Exception AsException(string fullName) => new FileNotFoundException($"Could not find file: {fullName}", fullName);

        public ValueTask<Stream> OpenForReadAsync() => throw AsException(FullName);

        public ValueTask<bool> RequestWriteAccessAsync([NotNullWhen(true)] out IWritableFile? writableFile)
        {
            writableFile = default;
            return ValueTask.FromResult(false);
        }

        public ValueTask<Stream> OpenForWriteAsync() => throw AsException(FullName);

        public ValueTask<bool> DeleteAsync() => ValueTask.FromResult(false);
    }
}
