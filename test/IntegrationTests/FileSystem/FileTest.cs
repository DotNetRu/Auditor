using System;
using System.IO;
using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.FileSystem;
using Xunit;

namespace DotNetRu.Auditor.IntegrationTests.FileSystem
{
    [Collection(TempFileSystemDependency.Name)]
    public sealed class FileTest
    {
        private readonly IFile file;

        public FileTest(FileSystemFixture fileSystem)
        {
            file = Initialize(fileSystem.PhysicalRoot);
        }

        [Fact]
        public async Task ShouldRead()
        {
            // Arrange
            const string content = "abc 123";
            await WriteAsync(content).ConfigureAwait(false);

            await using var fileStream = await file.OpenForReadAsync().ConfigureAwait(false);
            using var reader = new StreamReader(fileStream);

            // Act
            var actualContent = await reader.ReadLineAsync().ConfigureAwait(false);

            // Assert
            Assert.Equal(content, actualContent);
        }

        [Fact]
        public async Task ShouldRaiseErrorWhenReadNonExistent()
        {
            // Arrange
            var fileExists = await file.ExistsAsync().ConfigureAwait(false);
            Assert.False(fileExists);

            // Act
            async Task<Stream> ReadNonExistentAsync() => await file.OpenForReadAsync().ConfigureAwait(false);

            // Assert
            Assert.Throws<FileNotFoundException>(() => ReadNonExistentAsync().GetAwaiter().GetResult());
        }

        [Fact]
        public async Task ShouldWrite()
        {
            // Arrange
            var fileExists = await file.ExistsAsync().ConfigureAwait(false);
            Assert.False(fileExists);

            // Act
            await WriteAsync("123").ConfigureAwait(false);

            // Assert
            fileExists = await file.ExistsAsync().ConfigureAwait(false);
            Assert.True(fileExists);
        }

        [Fact]
        public async Task ShouldDelete()
        {
            // Arrange
            await WriteAsync("123").ConfigureAwait(false);

            var fileExists = await file.ExistsAsync().ConfigureAwait(false);
            Assert.True(fileExists);

            var writable = await GetWritable().ConfigureAwait(false);

            // Act
            var wasDeleted = await writable.DeleteAsync().ConfigureAwait(false);

            // Assert
            Assert.True(wasDeleted);

            fileExists = await file.ExistsAsync().ConfigureAwait(false);
            Assert.False(fileExists);
        }

        [Fact]
        public async Task ShouldReturnFalseWhenDeleteNonExistent()
        {
            // Arrange
            var fileExists = await file.ExistsAsync().ConfigureAwait(false);
            Assert.False(fileExists);

            var writable = await GetWritable().ConfigureAwait(false);

            // Act
            var wasDeleted = await writable.DeleteAsync().ConfigureAwait(false);

            // Assert
            Assert.False(wasDeleted);
        }

        private async Task<IWritableFile> GetWritable()
        {
            var writableFile = await file.RequestWriteAccessAsync().ConfigureAwait(false);
            return AssertEx.NotNull(writableFile);
        }

        private async Task WriteAsync(string content)
        {
            var writable = await GetWritable().ConfigureAwait(false);

            await using var fileStream = await writable.OpenForWriteAsync().ConfigureAwait(false);
            await using var fileWriter = new StreamWriter(fileStream);

            await fileWriter.WriteAsync(content).ConfigureAwait(false);
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
