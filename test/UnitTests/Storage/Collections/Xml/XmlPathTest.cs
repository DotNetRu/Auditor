using DotNetRu.Auditor.Storage.Collections.Xml;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Storage.Collections.Xml
{
    public sealed class XmlPathTest
    {
        [Theory]
        [InlineData("index.txt", "index.xml")]
        [InlineData("index.xml", "index.xml")]
        [InlineData("index", "index.xml")]
        public void ShouldChangeExtensionToXml(string path, string expectedPath)
        {
            // Act
            var actualPath = XmlPath.ChangeExtension(path);

            // Assert
            Assert.Equal(expectedPath, actualPath);
        }

        [Fact]
        public void ShouldHaveXmlExtension()
        {
            // Act
            var hasExtension = XmlPath.HasExtension("index.xml");

            // Assert
            Assert.True(hasExtension);
        }

        [Fact]
        public void ShouldHaveNoXmlExtension()
        {
            // Act
            var hasExtension = XmlPath.HasExtension("index.txt");

            // Assert
            Assert.False(hasExtension);
        }
    }
}
