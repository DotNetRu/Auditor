using System.IO;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.Storage.FileSystem.Physical
{
    public sealed class PhysicalFile : FileSystemEntry, IFile
    {
        public PhysicalFile(string fullName)
            : base(fullName, File.Exists(fullName))
        {
        }

        public ValueTask<Stream> OpenForReadAsync()
        {
            if (!Exists)
            {
                throw NotFoundFile.ToException(FullName);
            }

            var inputStream = File.OpenRead(FullName);
            return ValueTask.FromResult<Stream>(inputStream);
        }

        public ValueTask<Stream> OpenForWriteAsync()
        {
            if (!Exists)
            {
                throw NotFoundFile.ToException(FullName);
            }

            var outputStream = File.OpenWrite(FullName);
            return ValueTask.FromResult<Stream>(outputStream);
        }
    }
}
