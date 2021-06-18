using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.Storage.FileSystem.Physical
{
    public class PhysicalDirectory : FileSystemEntry, IDirectory
    {
        public PhysicalDirectory(string fullName)
            : base(fullName, Directory.Exists(fullName))
        {
        }

        public ValueTask<IDirectory> GetDirectoryInfoAsync(string subPath)
        {
            if (!Exists)
            {
                return NotFoundDirectory.ToTask(subPath);
            }

            var fullDirectoryName = GetFullPath(subPath);
            var directory = new PhysicalDirectory(fullDirectoryName);
            return ValueTask.FromResult<IDirectory>(directory);
        }

        public ValueTask<IFile> GetFileInfoAsync(string subPath)
        {
            if (!Exists)
            {
                return NotFoundFile.ToTask(subPath);
            }

            var fullFileName = GetFullPath(subPath);
            var file = new PhysicalFile(fullFileName);
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

        public ValueTask<IFile> CreateFileAsync(string subPath)
        {
            if (!Exists)
                return NotFoundFile.ToTask(subPath);

            var createdFile = new PhysicalFile(subPath);
            using var fileStream = File.Create(createdFile.FullName);
            return ValueTask.FromResult<IFile>(createdFile);
        }
    }
}
