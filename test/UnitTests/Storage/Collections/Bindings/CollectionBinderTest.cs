using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Storage.Collections;
using DotNetRu.Auditor.Storage.Collections.Bindings;
using DotNetRu.Auditor.Storage.FileSystem;
using DotNetRu.Auditor.Storage.IO;
using Moq;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Storage.Collections.Bindings
{
    public sealed class CollectionBinderTest
    {
        private readonly IReadOnlyList<string> actualCollections;
        private readonly List<string> actualFiles = new();
        private readonly List<string> actualDirectories = new();


        public CollectionBinderTest()
        {
            actualCollections = ScanAsync().GetAwaiter().GetResult();
        }

        [Fact]
        public void ShouldVisitAllCollectionDirectories()
        {
            // Arrange
            var expectedCollections = new[] { "A", "B" };

            // Assert
            AssertEx.Equivalence(expectedCollections, actualCollections);
        }

        [Fact]
        public void ShouldVisitAllXmlDirectories()
        {
            // Arrange
            var expectedDirectories = new[] { "a1", "a2", "a3" };

            // Assert
            AssertEx.Equivalence(expectedDirectories, actualDirectories);
        }

        [Fact]
        public void ShouldVisitAllXmlFiles()
        {
            // Arrange
            var expectedFiles = new[] { "b1.xml", "b2.xml", "b3.xml" };

            // Assert
            AssertEx.Equivalence(expectedFiles, actualFiles);
        }

        [Fact]
        public async Task ShouldSkipNullCollection()
        {
            // Arrange
            var binder = new CollectionBinder(() => new MeasureMatcher(this, true));
            var databaseDirectory = await CreateDatabaseDirectoryAsync().ConfigureAwait(false);

            // Act
            var collections = await binder.ScanAsync(databaseDirectory).ToListAsync();

            // Assert
            Assert.Empty(collections);
        }

        private static async Task<IDirectory> CreateDatabaseDirectoryAsync()
        {
            // A --→ a1 -→ index.xml
            //   |-→ a2 -→ index.xml
            //   |-→ a3 -→ index.xml
            //
            // B --→ b1.xml
            //   |-→ b2.xml
            //   |-→ b3.xml

            var root = MemoryFileSystem.ForDirectory(AbsolutePath.Root.FullName);
            var a = root.GetDirectory("A");
            var b = root.GetDirectory("B");

            await Enumerable.Empty<Task>()
                .Concat(new[] { "a1", "a2", "a3" }.Select(id => a.WriteToXmlDirectoryCollectionAsync(id)))
                .Concat(new[] { "b1", "b2", "b3" }.Select(id => b.WriteToXmlFileCollectionAsync(id)))
                .WhenAll()
                .ConfigureAwait(false);

            return root;
        }

        private async Task<IReadOnlyList<string>> ScanAsync()
        {
            var binder = new CollectionBinder(CreateMatcher);
            var databaseDirectory = await CreateDatabaseDirectoryAsync().ConfigureAwait(false);
            var collections = await binder.ScanAsync(databaseDirectory).ToListAsync();
            var collectionNames = collections.Select(collection => collection.Name);
            return collectionNames.ToList();
        }

        private Matcher CreateMatcher()
        {
            return new MeasureMatcher(this);
        }

        private sealed class MeasureMatcher : Matcher
        {
            private readonly CollectionBinderTest collector;
            private readonly bool doNotMatch;

            public MeasureMatcher(CollectionBinderTest collector, bool doNotMatch = false)
            {
                this.collector = collector;
                this.doNotMatch = doNotMatch;
            }

            public override Task AcceptAsync(IFile file)
            {
                collector.actualFiles.Add(file.Name);
                return Task.CompletedTask;
            }

            public override Task AcceptAsync(IDirectory directory)
            {
                collector.actualDirectories.Add(directory.Name);
                return Task.CompletedTask;
            }

            public override IDocumentCollection? Match(IDirectory collectionDirectory)
            {
                if (doNotMatch)
                {
                    return default;
                }

                var collection = new Mock<IDocumentCollection>(MockBehavior.Strict);
                collection.Setup(c => c.Name).Returns(collectionDirectory.Name);
                return collection.Object;
            }
        }
    }
}
