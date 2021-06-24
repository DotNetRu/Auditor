using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.FileSystem;
using Xunit;

namespace DotNetRu.Auditor.IntegrationTests.PhysicalFileSystem
{
    [Collection(TempFileSystemDependency.Name)]
    public sealed class PhysicalDirectoryTest
    {
        private readonly IDirectory root;

        public PhysicalDirectoryTest(TempFileSystemFixture fileSystem)
        {
            root = fileSystem.Root;
        }

        [Fact]
        public async Task ShouldExists()
        {
            // Act
            var exists = await root.ExistsAsync().ConfigureAwait(false);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ShouldEnumerateTopLevelDirectories()
        {
            // Arrange
            var expectedDirectories = new[] { "A", "B", "C" };
            var actualDirectories = new List<string>();
            var actualExists = new List<bool>();

            // Act
            await foreach (var directory in root.EnumerateDirectoriesAsync().ConfigureAwait(false))
            {
                actualDirectories.Add(directory.Name);

                var exists = await directory.ExistsAsync().ConfigureAwait(false);
                actualExists.Add(exists);
            }

            // Assert
            AssertEx.Equivalence(expectedDirectories, actualDirectories);
            Assert.All(actualExists, Assert.True);
        }

        [Fact]
        public async Task ShouldEnumerateFiles()
        {
            // Arrange
            var expectedFiles = new[] { "b20.txt", "b21.txt" };
            var actualFiles = new List<string>();
            var actualExists = new List<bool>();

            var directory = await root.GetDirectoryAsync(Path.Combine("B", "B2")).ConfigureAwait(false);

            // Act
            await foreach (var file in directory.EnumerateFilesAsync().ConfigureAwait(false))
            {
                actualFiles.Add(file.Name);

                var exists = await file.ExistsAsync().ConfigureAwait(false);
                actualExists.Add(exists);
            }

            // Assert
            AssertEx.Equivalence(expectedFiles, actualFiles);
            Assert.All(actualExists, Assert.True);
        }

        [Fact]
        public async Task ShouldGetDirectoryByName()
        {
            // Arrange
            var nestedName = Path.Combine("A", "A2");

            // Act
            var directory = await root.GetDirectoryAsync(nestedName).ConfigureAwait(false);

            // Assert
            Assert.Equal("A2", directory.Name);

            var exists = await directory.ExistsAsync().ConfigureAwait(false);
            Assert.True(exists);
        }

        [Fact]
        public async Task ShouldGetFileByName()
        {
            // Arrange
            var directory = await root.GetDirectoryAsync(Path.Combine("A", "A2")).ConfigureAwait(false);

            // Act
            var file = await directory.GetFileAsync("a20.txt").ConfigureAwait(false);

            // Assert
            Assert.Equal("a20.txt", file.Name);

            var exists = await file.ExistsAsync().ConfigureAwait(false);
            Assert.True(exists);
        }

        [Fact]
        public async Task ShouldGetDirectoryByNameWhenNotExists()
        {
            // Arrange
            const string? name = "non-existent-directory";

            // Act
            var directory = await root.GetDirectoryAsync(name).ConfigureAwait(false);

            // Assert
            Assert.Equal(name, directory.Name);

            var exists = await directory.ExistsAsync().ConfigureAwait(false);
            Assert.False(exists);
        }

        [Fact]
        public async Task ShouldGetFileByNameWhenNotExists()
        {
            // Arrange
            const string name = "non-existent-file";

            // Act
            var file = await root.GetFileAsync(name).ConfigureAwait(false);

            // Assert
            Assert.Equal(name, file.Name);

            var exists = await file.ExistsAsync().ConfigureAwait(false);
            Assert.False(exists);
        }
    }
}
