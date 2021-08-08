using System.Threading.Tasks;
using DotNetRu.Auditor.Data.Description;
using DotNetRu.Auditor.Storage.Collections;
using DotNetRu.Auditor.Storage.Collections.Xml;
using DotNetRu.Auditor.Storage.FileSystem;
using Moq;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Storage.Collections.Xml
{
    public sealed class XmlFileMatcherTest
    {
        private readonly XmlFileMatcher matcher;
        private readonly IDirectory directory;
        private readonly IDocumentCollection expectedCollection;

        public XmlFileMatcherTest()
        {
            var registry = ModelRegistry.Instance;
            var collectionName = registry.GetCommunityCollectionName();
            directory = Mocker.MockDirectory(collectionName).Object;

            expectedCollection = new Mock<IDocumentCollection>(MockBehavior.Strict).Object;

            var factory = new Mock<IXmlCollectionFactory>(MockBehavior.Strict);
            factory.Setup(f => f.Create(CollectionStructure.File, directory)).Returns(expectedCollection);

            matcher = new XmlFileMatcher(factory.Object);

            Assert.Null(matcher.ErrorMessage);
        }

        [Fact]
        public async Task ShouldHaveErrorWhenSomeDirectoryAccepts()
        {
            // Arrange
            const string fullName = "etc";
            var someDirectory = Mocker.MockDirectory(fullName);

            // Act
            await matcher.AcceptAsync(someDirectory.Object).ConfigureAwait(false);

            // Assert
            var error = AssertEx.NotNull(matcher.ErrorMessage);
            Assert.Null(matcher.Match(directory));
            Assert.Contains(fullName, error);
            Assert.Contains("can't contain directory", error);
        }

        [Fact]
        public async Task ShouldHaveErrorWhenNotExistingFileAccepts()
        {
            // Arrange
            const string fullName = "index.xml";
            var notExistingFile = Mocker.MockFile(fullName, false);

            // Act
            await matcher.AcceptAsync(notExistingFile.Object).ConfigureAwait(false);

            // Assert
            var error = AssertEx.NotNull(matcher.ErrorMessage);
            Assert.Null(matcher.Match(directory));
            Assert.Contains(fullName, error);
            Assert.Contains("not found", error);
        }

        [Fact]
        public async Task ShouldHaveErrorWhenNotXmlFileAccepts()
        {
            // Arrange
            const string fullName = "index.txt";
            var notXmlFile = Mocker.MockFile(fullName);

            // Act
            await matcher.AcceptAsync(notXmlFile.Object).ConfigureAwait(false);

            // Assert
            var error = AssertEx.NotNull(matcher.ErrorMessage);
            Assert.Null(matcher.Match(directory));
            Assert.Contains(fullName, error);
            Assert.Contains("format", error);
        }

        [Fact]
        public void ShouldHaveErrorWhenNoFileAccepts()
        {
            // Act
            var collection = matcher.Match(directory);

            // Assert
            Assert.Null(collection);
            var error = AssertEx.NotNull(matcher.ErrorMessage);
            Assert.Contains("No files", error);
        }

        [Fact]
        public async Task ShouldHaveErrorWhenUnknownFileAccepts()
        {
            // Arrange
            const string fullName = "index.xml";
            var existingXmlFile = Mocker.MockFile(fullName);

            var factory = new Mock<IXmlCollectionFactory>(MockBehavior.Strict);
            factory.Setup(f => f.Create(CollectionStructure.File, directory)).Returns(default(IDocumentCollection?));

            var localMatcher = new XmlFileMatcher(factory.Object);
            await localMatcher.AcceptAsync(existingXmlFile.Object).ConfigureAwait(false);

            // Act
            var collection = localMatcher.Match(directory);

            // Assert
            Assert.Null(collection);
            var error = AssertEx.NotNull(localMatcher.ErrorMessage);
            Assert.Contains("Unknown model", error);
        }

        [Fact]
        public async Task ShouldHaveNoErrorWhenExistingXmlFileAccepts()
        {
            // Arrange
            const string fullName = "index.xml";
            var existingXmlFile = Mocker.MockFile(fullName);

            // Act
            await matcher.AcceptAsync(existingXmlFile.Object).ConfigureAwait(false);

            // Assert
            Assert.Null(matcher.ErrorMessage);
            var collection = AssertEx.NotNull(matcher.Match(directory));
            Assert.Same(expectedCollection, collection);
        }
    }
}
