using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.Storage.FileSystem.Physical
{
    public sealed class PhysicalFile : FileSystemEntry, IWritableFile
    {
        public PhysicalFile(string fullName)
            : base(fullName)
        {
        }

        public override ValueTask<bool> ExistsAsync() => ValueTask.FromResult(Exists);

        public ValueTask<Stream> OpenForReadAsync()
        {
            if (!Exists)
            {
                throw FileNotFound();
            }

            var inputStream = File.OpenRead(FullName);
            return ValueTask.FromResult<Stream>(inputStream);
        }

        public ValueTask<bool> RequestWriteAccessAsync([NotNullWhen(true)] out IWritableFile? writableFile)
        {
            // TODO: Test real file permissions
            writableFile = this;
            return ValueTask.FromResult(true);
        }

        public ValueTask<Stream> OpenForWriteAsync()
        {
            // TDO: Create full path hierarchy
            var outputStream = File.OpenWrite(FullName);
            return ValueTask.FromResult<Stream>(outputStream);
        }

        public ValueTask<bool> DeleteAsync()
        {
            if (!Exists)
            {
                return ValueTask.FromResult(false);
            }

            try
            {
                File.Delete(FullName);
            }
            catch (Exception)
            {
                // Result will be checked later
            }

            return ValueTask.FromResult(!Exists);
        }

        private bool Exists => File.Exists(FullName);
    }
}
