using System;
using System.IO;
using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.FileSystem;
using Xunit;

namespace DotNetRu.Auditor.IntegrationTests.FileSystem
{
    public sealed class FileSystemFixture : IDisposable
    {
        private readonly TempFileSystem temp;

        public FileSystemFixture()
        {
            temp = TempFileSystem.Create();
            PhysicalRoot = PhysicalFileSystem.ForDirectory(temp.Root);

            InitializeAsync().GetAwaiter().GetResult();
        }

        public IDirectory PhysicalRoot { get; }

        public void Dispose()
        {
            temp.Dispose();
        }

        private async Task InitializeAsync()
        {
            // A --→ A1 -→ a10.txt
            //   |-→ A2 -→ a20.txt
            //   |-→ A3 -→ a30.txt
            //
            // B --→ B1 -→ b10.txt
            //   |-→ B2 --→ b20.txt
            //          |-→ b21.txt
            //
            // C -→ c0.txt

            await CreateFileAsync(PhysicalRoot, Path.Combine("A", "A1", "a10.txt")).ConfigureAwait(false);
            await CreateFileAsync(PhysicalRoot, Path.Combine("A", "A2", "a20.txt")).ConfigureAwait(false);
            await CreateFileAsync(PhysicalRoot, Path.Combine("A", "A3", "a30.txt")).ConfigureAwait(false);
            await CreateFileAsync(PhysicalRoot, Path.Combine("B", "B1", "b10.txt")).ConfigureAwait(false);
            await CreateFileAsync(PhysicalRoot, Path.Combine("B", "B2", "b20.txt")).ConfigureAwait(false);
            await CreateFileAsync(PhysicalRoot, Path.Combine("B", "B2", "b21.txt")).ConfigureAwait(false);
            await CreateFileAsync(PhysicalRoot, Path.Combine("C", "c0.txt")).ConfigureAwait(false);
        }

        private static async ValueTask<IWritableFile> CreateFileAsync(IDirectory root, string relativeFilePath)
        {
            var fileParentDirectoryName = Path.GetDirectoryName(relativeFilePath);
            var fileName = Path.GetFileName(relativeFilePath);
            var directory = root;

            if (fileParentDirectoryName != null)
            {
                directory = root.GetDirectory(fileParentDirectoryName);
            }

            var file = directory.GetFile(fileName);
            var fileExists = await file.ExistsAsync().ConfigureAwait(false);
            Assert.False(fileExists);

            var writableFile = await file.RequestWriteAccessAsync().ConfigureAwait(false);
            writableFile = AssertEx.NotNull(writableFile);

            await using (var fileStream = await writableFile.OpenForWriteAsync().ConfigureAwait(false))
            await using (var fileWriter = new StreamWriter(fileStream))
            {
                await fileWriter.WriteAsync(file.FullName).ConfigureAwait(false);
            }

            fileExists = await file.ExistsAsync().ConfigureAwait(false);
            Assert.True(fileExists);

            return writableFile;
        }
    }

    [CollectionDefinition(Name)]
    public sealed class TempFileSystemDependency : ICollectionFixture<FileSystemFixture>
    {
        public const string Name = "TempFileSystem";
    }
}
