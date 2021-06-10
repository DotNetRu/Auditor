using System.IO;

namespace DotNetRu.Auditor.Storage.FileSystem.Physical
{
    public sealed class PhysicalFile : FileSystemEntry, IFile
    {
        public PhysicalFile(string fullName)
            : base(fullName, File.Exists(fullName))
        {
        }

        public Stream OpenForRead()
        {
            if (!Exists)
            {
                throw NotFoundFile.ToException(FullName);
            }

            return File.OpenRead(FullName);
        }

        public Stream OpenForWrite()
        {
            if (!Exists)
            {
                throw NotFoundFile.ToException(FullName);
            }

            return File.OpenWrite(FullName);
        }
    }
}
