using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.Storage.FileSystem.Physical
{
    public class PhysicalDirectory : FileSystemEntry, IDirectory
    {
        public PhysicalDirectory(string fullName)
            : base(fullName)
        {
        }

        public override ValueTask<bool> ExistsAsync() => ValueTask.FromResult(Exists);

        public ValueTask<IDirectory> GetDirectoryAsync(string childDirectoryName)
        {
            var fullChildName = GetFullChildPath(childDirectoryName);
            var directory = new PhysicalDirectory(fullChildName);
            return ValueTask.FromResult<IDirectory>(directory);
        }

        public ValueTask<IFile> GetFileAsync(string childFileName)
        {
            var fullChildName = GetFullChildPath(childFileName);
            var file = new PhysicalFile(fullChildName);
            return ValueTask.FromResult<IFile>(file);
        }

        public async IAsyncEnumerable<IDirectory> EnumerateDirectoriesAsync()
        {
            if (!Exists)
            {
                yield break;
            }

            await Task.Yield();

            foreach (var directoryFullName in Directory.EnumerateDirectories(FullName))
            {
                yield return new PhysicalDirectory(directoryFullName);
            }
        }

        public async IAsyncEnumerable<IFile> EnumerateFilesAsync()
        {
            if (!Exists)
            {
                yield break;
            }

            await Task.Yield();

            foreach (var fileFullName in Directory.EnumerateFiles(FullName))
            {
                yield return new PhysicalFile(fileFullName);
            }
        }

        private bool Exists => Directory.Exists(FullName);
    }
}
