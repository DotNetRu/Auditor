using System;
using System.Linq;
using DotNetRu.Auditor.Data.Model;
using DotNetRu.Auditor.Storage.Sessions;
using DotNetRu.Auditor.UnitTests.Storage.Collections.Xml;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Storage.Sessions
{
    public sealed class IdentityMapTest
    {
        private const string KnownId = nameof(KnownId);
        private readonly IdentityMap<Community> map = new();

        [Fact]
        public void ShouldReturnTheSameInstanceWhenRegisterTwice()
        {
            // Arrange
            var document = Mocker.Community(KnownId);

            // Act
            var firstRegistered = map.RegisterOrigin(document);
            var secondRegistered = map.RegisterOrigin(document);

            // Assert
            Assert.Same(firstRegistered, secondRegistered);
        }

        [Fact]
        public void ShouldReturnTheSameInstanceWhenRegistered()
        {
            // Arrange
            var document = Mocker.Community(KnownId);
            var registeredDocument = map.RegisterOrigin(document);

            // Act
            var found = map.TryGet(KnownId, out var resolvedDocument);

            // Assert
            Assert.True(found);
            Assert.Same(registeredDocument, resolvedDocument);
        }

        [Fact]
        public void ShouldReturnDefaultWhenNotRegistered()
        {
            // Arrange
            const string unknownId = nameof(ShouldReturnDefaultWhenNotRegistered);

            // Act
            var found = map.TryGet(unknownId, out var resolvedDocument);

            // Assert
            Assert.False(found);
            Assert.Null(resolvedDocument);
        }

        [Fact]
        public void ShouldReturnMany()
        {
            // Arrange
            var id1 = KnownId + 1;
            var id2 = KnownId + 2;
            var id3 = KnownId + 3;
            var registeredDocument1 = map.RegisterOrigin(Mocker.Community(id1));
            var registeredDocument2 = map.RegisterOrigin(Mocker.Community(id2));
            var ids = new[] { id2, id1, id3 };

            // Act
            var documents = map.GetDocuments(ids);

            // Assert
            Assert.Equal(2, documents.Count);

            Assert.Same(registeredDocument1, documents[id1]);
            Assert.Same(registeredDocument2, documents[id2]);

            Assert.DoesNotContain(id3, documents.Keys);
        }

        [Fact]
        public void ShouldReturnTheSameInstanceWhenNewRegistered()
        {
            // Arrange
            var document = Mocker.Community(KnownId);
            map.RegisterNew(document);

            // Act
            var found = map.TryGet(KnownId, out var resolvedDocument);

            // Assert
            Assert.True(found);
            Assert.Same(document, resolvedDocument);
        }

        [Fact]
        public void ShouldRaiseErrorWhenRegisterNewTwice()
        {
            // Arrange
            var document = Mocker.Community(KnownId);
            map.RegisterNew(document);

            // Act
            void RegisterTwice() => map.RegisterNew(document);

            // Assert
            Assert.Throws<ArgumentException>(RegisterTwice);
        }

        [Fact]
        public void ShouldReturnChangedOrAddedDocuments()
        {
            // Arrange
            var id1 = KnownId + 1;
            var id2 = KnownId + 2;
            var id3 = KnownId + 3;
            var id4 = KnownId + 4;
            var id5 = KnownId + 5;

            map.RegisterOrigin(Mocker.Community(id1));
            var registeredDocument2 = map.RegisterOrigin(Mocker.Community(id2));
            map.RegisterOrigin(Mocker.Community(id3));
            var registeredDocument4 = map.RegisterOrigin(Mocker.Community(id4));
            var newDocument5 = Mocker.Community(id5);
            map.RegisterNew(newDocument5);

            // Act
            registeredDocument2.Name += " (Modified 2)";
            registeredDocument4.Name += " (Modified 4)";

            var changedDocuments = map.PopChanges();

            // Assert
            Assert.Equal(3, changedDocuments.Count);

            var modifiedDocument2 = Assert.Single(changedDocuments.Where(d => d.Id == id2));
            Assert.Same(registeredDocument2, modifiedDocument2);

            var modifiedDocument4 = Assert.Single(changedDocuments.Where(d => d.Id == id4));
            Assert.Same(registeredDocument4, modifiedDocument4);

            var addedDocument5 = Assert.Single(changedDocuments.Where(d => d.Id == id5));
            Assert.Same(newDocument5, addedDocument5);

            Assert.Equal(0, map.Count);
        }
    }
}
