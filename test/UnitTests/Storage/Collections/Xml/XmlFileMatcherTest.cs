using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.Collections.Xml;
using Xunit;

using static DotNetRu.Auditor.UnitTests.Storage.Collections.Xml.Mocker;

namespace DotNetRu.Auditor.UnitTests.Storage.Collections.Xml
{
    public sealed class XmlFileMatcherTest
    {
        private readonly XmlFileMatcher matcher;
        private const string RootName = "Root";

        public XmlFileMatcherTest()
        {
            var directory = MockDirectory(RootName);
            matcher = new XmlFileMatcher(directory.Object);

            Assert.Null(matcher.ErrorMessage);
        }

        [Fact]
        public async Task ShouldHaveErrorWhenSomeDirectoryAccepts()
        {
            // Arrange
            const string fullName = "etc";
            var someDirectory = MockDirectory(fullName);

            // Act
            await matcher.AcceptAsync(someDirectory.Object).ConfigureAwait(false);

            // Assert
            var error = AssertEx.NotNull(matcher.ErrorMessage);
            Assert.Null(matcher.Match());
            Assert.Contains(fullName, error);
            Assert.Contains("can't contain directory", error);
        }

        [Fact]
        public async Task ShouldHaveErrorWhenNotExistingFileAccepts()
        {
            // Arrange
            const string fullName = "index.xml";
            var notExistingFile = MockFile(fullName, false);

            // Act
            await matcher.AcceptAsync(notExistingFile.Object).ConfigureAwait(false);

            // Assert
            var error = AssertEx.NotNull(matcher.ErrorMessage);
            Assert.Null(matcher.Match());
            Assert.Contains(fullName, error);
            Assert.Contains("not found", error);
        }

        [Fact]
        public async Task ShouldHaveErrorWhenNotXmlFileAccepts()
        {
            // Arrange
            const string fullName = "index.txt";
            var notXmlFile = MockFile(fullName);

            // Act
            await matcher.AcceptAsync(notXmlFile.Object).ConfigureAwait(false);

            // Assert
            var error = AssertEx.NotNull(matcher.ErrorMessage);
            Assert.Null(matcher.Match());
            Assert.Contains(fullName, error);
            Assert.Contains("format", error);
        }

        [Fact]
        public void ShouldHaveErrorWhenNoFileAccepts()
        {
            // Act
            var collection = matcher.Match();

            // Assert
            Assert.Null(collection);
            var error = AssertEx.NotNull(matcher.ErrorMessage);
            Assert.Contains("No files", error);
        }

        [Fact]
        public async Task ShouldHaveNoErrorWhenExistingXmlFileAccepts()
        {
            // Arrange
            const string fullName = "index.xml";
            var existingXmlFile = MockFile(fullName);

            // Act
            await matcher.AcceptAsync(existingXmlFile.Object).ConfigureAwait(false);

            // Assert
            Assert.Null(matcher.ErrorMessage);
            var collection = AssertEx.NotNull(matcher.Match());
            Assert.Equal(RootName, collection.Name);
        }
    }
}
