using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.FileSystem;
using Xunit;

namespace DotNetRu.Auditor.IntegrationTests.FileSystem
{
    [Collection(FileSystemFixture.Name)]
    public sealed class DirectoryTest
    {
        private readonly IReadOnlyList<IDirectory> directories;

        public DirectoryTest(FileSystemFixture fixture)
        {
            directories = fixture.AllRoots;
        }

        [Fact]
        public async Task ShouldExists()
        {
            foreach (var root in directories)
            {
                // Act
                var exists = await root.ExistsAsync().ConfigureAwait(false);

                // Assert
                Assert.True(exists);
            }
        }

        [Fact]
        public async Task ShouldEnumerateTopLevelDirectories()
        {
            foreach (var root in directories)
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
        }

        [Fact]
        public async Task ShouldEnumerateFiles()
        {
            foreach (var root in directories)
            {
                // Arrange
                var expectedFiles = new[] { "b20.txt", "b21.txt" };
                var actualFiles = new List<string>();
                var actualExists = new List<bool>();

                var directory = root.GetDirectory(Path.Combine("B", "B2"));

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
        }

        [Fact]
        public async Task ShouldGetDirectoryByName()
        {
            foreach (var root in directories)
            {
                // Arrange
                var nestedName = Path.Combine("A", "A2");

                // Act
                var directory = root.GetDirectory(nestedName);

                // Assert
                Assert.Equal("A2", directory.Name);

                var exists = await directory.ExistsAsync().ConfigureAwait(false);
                Assert.True(exists);
            }
        }

        [Fact]
        public async Task ShouldGetFileByName()
        {
            foreach (var root in directories)
            {
                // Arrange
                var directory = root.GetDirectory(Path.Combine("A", "A2"));

                // Act
                var file = directory.GetFile("a20.txt");

                // Assert
                Assert.Equal("a20.txt", file.Name);

                var exists = await file.ExistsAsync().ConfigureAwait(false);
                Assert.True(exists);
            }
        }

        [Fact]
        public async Task ShouldGetDirectoryByNameWhenNotExists()
        {
            foreach (var root in directories)
            {
                // Arrange
                const string? name = "non-existent-directory";

                // Act
                var directory = root.GetDirectory(name);

                // Assert
                Assert.Equal(name, directory.Name);

                var exists = await directory.ExistsAsync().ConfigureAwait(false);
                Assert.False(exists);
            }
        }

        [Fact]
        public async Task ShouldGetFileByNameWhenNotExists()
        {
            foreach (var root in directories)
            {
                // Arrange
                const string name = "non-existent-file";

                // Act
                var file = root.GetFile(name);

                // Assert
                Assert.Equal(name, file.Name);

                var exists = await file.ExistsAsync().ConfigureAwait(false);
                Assert.False(exists);
            }
        }
    }
}
