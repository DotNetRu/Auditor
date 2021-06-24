using System;
using System.IO;
using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.FileSystem;
using Xunit;

namespace DotNetRu.Auditor.IntegrationTests.PhysicalFileSystem
{
    public sealed class TempFileSystemFixture : IDisposable
    {
        private readonly TempFileSystem fileSystem;

        public TempFileSystemFixture()
        {
            fileSystem = TempFileSystem.Create();

            InitializeAsync().GetAwaiter().GetResult();
        }

        public IDirectory Root => fileSystem.Root;

        public void Dispose()
        {
            fileSystem.Dispose();
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

            await CreateFileAsync(Root, Path.Combine("A", "A1", "a10.txt")).ConfigureAwait(false);
            await CreateFileAsync(Root, Path.Combine("A", "A2", "a20.txt")).ConfigureAwait(false);
            await CreateFileAsync(Root, Path.Combine("A", "A3", "a30.txt")).ConfigureAwait(false);
            await CreateFileAsync(Root, Path.Combine("B", "B1", "b10.txt")).ConfigureAwait(false);
            await CreateFileAsync(Root, Path.Combine("B", "B2", "b20.txt")).ConfigureAwait(false);
            await CreateFileAsync(Root, Path.Combine("B", "B2", "b21.txt")).ConfigureAwait(false);
            await CreateFileAsync(Root, Path.Combine("C", "c0.txt")).ConfigureAwait(false);
        }

        private static async ValueTask<IWritableFile> CreateFileAsync(IDirectory root, string relativeFilePath)
        {
            var fileDirectory = Path.GetDirectoryName(relativeFilePath);
            var fileName = Path.GetFileName(relativeFilePath);
            var directory = root;

            if (fileDirectory != null)
            {
                directory = await root.GetDirectoryAsync(fileDirectory).ConfigureAwait(false);
            }

            var file = await directory.GetFileAsync(fileName).ConfigureAwait(false);
            var fileExists = await file.ExistsAsync().ConfigureAwait(false);
            Assert.False(fileExists);

            var fileCanWrite = await file.RequestWriteAccessAsync(out var fileHandler).ConfigureAwait(false);
            Assert.True(fileCanWrite);
            fileHandler = AssertEx.NotNull(fileHandler);

            await using (var fileStream = await fileHandler.OpenForWriteAsync().ConfigureAwait(false))
            await using (var fileWriter = new StreamWriter(fileStream))
            {
                await fileWriter.WriteAsync(file.FullName).ConfigureAwait(false);
            }

            fileExists = await file.ExistsAsync().ConfigureAwait(false);
            Assert.True(fileExists);

            return fileHandler;
        }
    }

    [CollectionDefinition(Name)]
    public sealed class TempFileSystemDependency : ICollectionFixture<TempFileSystemFixture>
    {
        public const string Name = "TempFileSystem";
    }
}
