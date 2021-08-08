using System.Threading.Tasks;
using DotNetRu.Auditor.Data.Description;
using DotNetRu.Auditor.Storage.Collections;
using DotNetRu.Auditor.Storage.Collections.Xml;
using DotNetRu.Auditor.Storage.FileSystem;
using Moq;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Storage.Collections.Xml
{
    public sealed class XmlDirectoryMatcherTest
    {
        private readonly XmlDirectoryMatcher matcher;
        private readonly IDirectory directory;
        private readonly IDocumentCollection expectedCollection;

        public XmlDirectoryMatcherTest()
        {
            var registry = ModelRegistry.Instance;
            var collectionName = registry.GetCommunityCollectionName();
            directory = Mocker.MockDirectory(collectionName).Object;

            expectedCollection = new Mock<IDocumentCollection>(MockBehavior.Strict).Object;

            var factory = new Mock<IXmlCollectionFactory>(MockBehavior.Strict);
            factory.Setup(f => f.Create(CollectionStructure.Directory, directory)).Returns(expectedCollection);

            matcher = new XmlDirectoryMatcher(factory.Object);

            Assert.Null(matcher.ErrorMessage);
        }

        [Fact]
        public async Task ShouldHaveErrorWhenSomeFileAccepts()
        {
            // Arrange
            const string fileName = "index.xml";
            var someFile = Mocker.MockFile(fileName);

            // Act
            await matcher.AcceptAsync(someFile.Object).ConfigureAwait(false);

            // Assert
            var error = AssertEx.NotNull(matcher.ErrorMessage);
            Assert.Null(matcher.Match(directory));
            Assert.Contains(fileName, error);
            Assert.Contains("can't contain files", error);
        }

        [Fact]
        public async Task ShouldHaveErrorWhenNotExistingFileAccepts()
        {
            // Arrange
            const string dirName = "etc";
            const string fileName = XmlPath.IndexFileName;
            var notExistingFile = Mocker.MockFile(fileName, false);

            var childDirectory = Mocker.MockDirectory(dirName);
            childDirectory.Setup(d => d.GetFile(fileName)).Returns(notExistingFile.Object);

            // Act
            await matcher.AcceptAsync(childDirectory.Object).ConfigureAwait(false);

            // Assert
            var error = AssertEx.NotNull(matcher.ErrorMessage);
            Assert.Null(matcher.Match(directory));
            Assert.Contains(dirName, error);
            Assert.Contains("not found", error);
        }

        [Fact]
        public void ShouldHaveErrorWhenNoDirectoryAccepts()
        {
            // Act
            var collection = matcher.Match(directory);

            // Assert
            Assert.Null(collection);
            var error = AssertEx.NotNull(matcher.ErrorMessage);
            Assert.Contains("No directories", error);
        }

        [Fact]
        public async Task ShouldHaveErrorWhenUnknownDirectoryAccepts()
        {
            // Arrange
            const string dirName = "etc";
            const string fileName = XmlPath.IndexFileName;
            var existingXmlFile = Mocker.MockFile(fileName);

            var childDirectory = Mocker.MockDirectory(dirName);
            childDirectory.Setup(d => d.GetFile(fileName)).Returns(existingXmlFile.Object);

            var factory = new Mock<IXmlCollectionFactory>(MockBehavior.Strict);
            factory.Setup(f => f.Create(CollectionStructure.Directory, directory)).Returns(default(IDocumentCollection?));

            var localMatcher = new XmlDirectoryMatcher(factory.Object);
            await localMatcher.AcceptAsync(childDirectory.Object).ConfigureAwait(false);

            // Act
            var collection = localMatcher.Match(directory);

            // Assert
            Assert.Null(collection);
            var error = AssertEx.NotNull(localMatcher.ErrorMessage);
            Assert.Contains("Unknown model", error);
        }

        [Fact]
        public async Task ShouldHaveNoErrorWhenExistingXmlDirectoryAccepts()
        {
            // Arrange
            const string dirName = "etc";
            const string fileName = XmlPath.IndexFileName;
            var existingXmlFile = Mocker.MockFile(fileName);

            var childDirectory = Mocker.MockDirectory(dirName);
            childDirectory.Setup(d => d.GetFile(fileName)).Returns(existingXmlFile.Object);

            // Act
            await matcher.AcceptAsync(childDirectory.Object).ConfigureAwait(false);

            // Assert
            Assert.Null(matcher.ErrorMessage);
            var collection = AssertEx.NotNull(matcher.Match(directory));
            Assert.Same(expectedCollection, collection);
        }
    }
}
