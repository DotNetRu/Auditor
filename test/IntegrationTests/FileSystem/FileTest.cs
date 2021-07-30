using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.FileSystem;
using DotNetRu.Auditor.UnitTests;
using Xunit;

namespace DotNetRu.Auditor.IntegrationTests.FileSystem
{
    [Collection(FileSystemFixture.Name)]
    public sealed class FileTest
    {
        private readonly IReadOnlyList<IFile> files;

        public FileTest(FileSystemFixture fixture)
        {
            files = fixture
                .AllRoots
                .Select(Initialize)
                .ToList();
        }

        [Fact]
        public async Task ShouldRead()
        {
            foreach (var file in files)
            {
                // Arrange
                const string content = "abc 123";
                await WriteAsync(file, content).ConfigureAwait(false);

                var fileStream = await file.OpenForReadAsync().ConfigureAwait(false);
                await using (fileStream.ConfigureAwait(false))
                {
                    // Act
                    var actualContent = await fileStream.ReadAllTextAsync().ConfigureAwait(false);

                    // Assert
                    Assert.Equal(content, actualContent);
                }
            }
        }

        [Fact]
        public async Task ShouldRaiseErrorWhenReadNonExistent()
        {
            foreach (var file in files)
            {
                // Arrange
                var fileExists = await file.ExistsAsync().ConfigureAwait(false);
                Assert.False(fileExists);

                // Act
                async Task<Stream> ReadNonExistentAsync() => await file.OpenForReadAsync().ConfigureAwait(false);

                // Assert
                Assert.Throws<FileNotFoundException>(() => ReadNonExistentAsync().GetAwaiter().GetResult());
            }
        }

        [Fact]
        public async Task ShouldWrite()
        {
            foreach (var file in files)
            {
                // Arrange
                var fileExists = await file.ExistsAsync().ConfigureAwait(false);
                Assert.False(fileExists);

                // Act
                await WriteAsync(file, "123").ConfigureAwait(false);

                // Assert
                fileExists = await file.ExistsAsync().ConfigureAwait(false);
                Assert.True(fileExists);
            }
        }

        [Fact]
        public async Task ShouldDelete()
        {
            foreach (var file in files)
            {
                // Arrange
                await WriteAsync(file, "123").ConfigureAwait(false);

                var fileExists = await file.ExistsAsync().ConfigureAwait(false);
                Assert.True(fileExists);

                var writable = await GetWritable(file).ConfigureAwait(false);

                // Act
                var wasDeleted = await writable.DeleteAsync().ConfigureAwait(false);

                // Assert
                Assert.True(wasDeleted);

                fileExists = await file.ExistsAsync().ConfigureAwait(false);
                Assert.False(fileExists);
            }
        }

        [Fact]
        public async Task ShouldReturnFalseWhenDeleteNonExistent()
        {
            foreach (var file in files)
            {
                // Arrange
                var fileExists = await file.ExistsAsync().ConfigureAwait(false);
                Assert.False(fileExists);

                var writable = await GetWritable(file).ConfigureAwait(false);

                // Act
                var wasDeleted = await writable.DeleteAsync().ConfigureAwait(false);

                // Assert
                Assert.False(wasDeleted);
            }
        }

        private static async Task<IWritableFile> GetWritable(IFile file)
        {
            var writableFile = await file.RequestWriteAccessAsync().ConfigureAwait(false);
            return AssertEx.NotNull(writableFile);
        }

        private static async Task WriteAsync(IFile file, string content)
        {
            var writable = await GetWritable(file).ConfigureAwait(false);

            var fileStream = await writable.OpenForWriteAsync().ConfigureAwait(false);
            await using (fileStream.ConfigureAwait(false))
            {
                var fileWriter = new StreamWriter(fileStream);
                await using (fileWriter.ConfigureAwait(false))
                {
                    await fileWriter.WriteAsync(content).ConfigureAwait(false);
                }
            }
        }

        private static IFile Initialize(IDirectory root)
        {
            static string Rand() => Guid.NewGuid().ToString("N");

            var directory = root.GetDirectory("C");
            var name = Path.Combine(Rand(), Rand(), Rand()) + ".test";
            var file = directory.GetFile(name);
            return file;
        }
    }
}
