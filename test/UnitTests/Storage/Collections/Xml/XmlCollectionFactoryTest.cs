using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Data.Description;
using DotNetRu.Auditor.Data.Model;
using DotNetRu.Auditor.Storage.Collections.Xml;
using DotNetRu.Auditor.Storage.FileSystem;
using Moq;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Storage.Collections.Xml
{
    public sealed class XmlCollectionFactoryTest
    {
        private readonly XmlCollectionFactory collectionFactory;

        public XmlCollectionFactoryTest()
        {
            var serializer = new Mock<IDocumentSerializer<Community>>(MockBehavior.Strict);

            var serializerFactory = new Mock<IDocumentSerializerFactory>(MockBehavior.Strict);
            serializerFactory.Setup(f => f.Create<Community>()).Returns(serializer.Object);

            collectionFactory = new XmlCollectionFactory(ModelRegistry.Instance, serializerFactory.Object);
        }

        [Fact]
        public void ShouldReturnDefaultWhenModelNotFound()
        {
            // Arrange
            var directory = new Mock<IDirectory>(MockBehavior.Strict);
            directory.Setup(d => d.Name).Returns("Unknown");

            // Act
            var collection = collectionFactory.Create(CollectionStructure.File, directory.Object);

            // Assert
            Assert.Null(collection);
        }

        [Fact]
        public void ShouldCreateFileCollection()
        {
            // Arrange
            var registry = ModelRegistry.Instance;
            var directory = new Mock<IDirectory>(MockBehavior.Strict);
            var collectionName = registry.GetCommunityCollectionName();
            directory.Setup(d => d.Name).Returns(collectionName);

            // Act
            var maybeCollection = collectionFactory.Create(CollectionStructure.File, directory.Object);

            // Assert
            var collection = AssertEx.NotNull(maybeCollection);
            Assert.IsType<XmlFileCollection<Community>>(collection);
            Assert.Equal(typeof(Community), collection.CollectionType);
            Assert.Equal(collectionName, collection.Name);
        }

        [Fact]
        public void ShouldCreateDirectoryCollection()
        {
            // Arrange
            var registry = ModelRegistry.Instance;
            var directory = new Mock<IDirectory>(MockBehavior.Strict);
            var collectionName = registry.GetCommunityCollectionName();
            directory.Setup(d => d.Name).Returns(collectionName);

            // Act
            var maybeCollection = collectionFactory.Create(CollectionStructure.Directory, directory.Object);

            // Assert
            var collection = AssertEx.NotNull(maybeCollection);
            Assert.IsType<XmlDirectoryCollection<Community>>(collection);
            Assert.Equal(typeof(Community), collection.CollectionType);
            Assert.Equal(collectionName, collection.Name);
        }

        [Theory]
        [InlineData("communities", "Communities")]
        [InlineData("meetups", "Meetups")]
        [InlineData("speakers", "Speakers")]
        [InlineData("talks", "Talks")]
        [InlineData("venues", "Venues")]
        [InlineData("friends", "Friends")]
        public void ShouldFormatCollectionName(string expectedCollectionName, string groupName)
        {
            // Act
            var actualCollectionName = XmlCollectionFactory.ToCollectionName(groupName);

            // Assert
            Assert.Equal(expectedCollectionName, actualCollectionName);
        }
    }
}
