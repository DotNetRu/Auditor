using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.Collections.Xml;
using Xunit;

using static DotNetRu.Auditor.UnitTests.Storage.Collections.Xml.Mocker;

namespace DotNetRu.Auditor.UnitTests.Storage.Collections.Xml
{
    public sealed class XmlDirectoryMatcherTest
    {
        private readonly XmlDirectoryMatcher matcher;
        private const string RootName = "Root";

        public XmlDirectoryMatcherTest()
        {
            var directory = MockDirectory(RootName);
            matcher = new XmlDirectoryMatcher(directory.Object);

            Assert.Null(matcher.ErrorMessage);
        }

        [Fact]
        public async Task ShouldHaveErrorWhenSomeFileAccepts()
        {
            // Arrange
            const string fileName = "index.xml";
            var someFile = MockFile(fileName);

            // Act
            await matcher.AcceptAsync(someFile.Object).ConfigureAwait(false);

            // Assert
            var error = AssertEx.NotNull(matcher.ErrorMessage);
            Assert.Null(matcher.Match());
            Assert.Contains(fileName, error);
            Assert.Contains("can't contain files", error);
        }

        [Fact]
        public async Task ShouldHaveErrorWhenNotExistingFileAccepts()
        {
            // Arrange
            const string dirName = "etc";
            const string fileName = XmlPath.IndexFileName;
            var notExistingFile = MockFile(fileName, false);

            var directory = MockDirectory(dirName);
            directory.Setup(d => d.GetFile(fileName)).Returns(notExistingFile.Object);

            // Act
            await matcher.AcceptAsync(directory.Object).ConfigureAwait(false);

            // Assert
            var error = AssertEx.NotNull(matcher.ErrorMessage);
            Assert.Null(matcher.Match());
            Assert.Contains(dirName, error);
            Assert.Contains("not found", error);
        }

        [Fact]
        public void ShouldHaveErrorWhenNoDirectoryAccepts()
        {
            // Act
            var collection = matcher.Match();

            // Assert
            Assert.Null(collection);
            var error = AssertEx.NotNull(matcher.ErrorMessage);
            Assert.Contains("No directories", error);
        }

        [Fact]
        public async Task ShouldHaveNoErrorWhenExistingXmlFileAccepts()
        {
            // Arrange
            const string dirName = "etc";
            const string fileName = XmlPath.IndexFileName;
            var existingXmlFile = MockFile(fileName);

            var directory = MockDirectory(dirName);
            directory.Setup(d => d.GetFile(fileName)).Returns(existingXmlFile.Object);

            // Act
            await matcher.AcceptAsync(directory.Object).ConfigureAwait(false);

            // Assert
            Assert.Null(matcher.ErrorMessage);
            var collection = AssertEx.NotNull(matcher.Match());
            Assert.Equal(RootName, collection.Name);
        }
    }
}
