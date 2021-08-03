using System;
using System.Linq;
using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Storage.Collections.Changes;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Storage.Collections.Changes
{
    public sealed class IdentityMapTest
    {
        private const string KnownId = nameof(KnownId);
        private readonly IdentityMap<Secret> map = new();

        [Fact]
        public void ShouldReturnTheSameInstanceWhenRegisterTwice()
        {
            // Arrange
            var document = new Secret(KnownId);

            // Act
            var firstRegistered = map.RegisterDocument(document);
            var secondRegistered = map.RegisterDocument(document);

            // Assert
            Assert.Same(firstRegistered, secondRegistered);
        }

        [Fact]
        public void ShouldReturnTheSameInstanceWhenRegistered()
        {
            // Arrange
            var document = new Secret(KnownId);
            var registeredDocument = map.RegisterDocument(document);

            // Act
            var found = map.TryResolveDocument(KnownId, out var resolvedDocument);

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
            var found = map.TryResolveDocument(unknownId, out var resolvedDocument);

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
            var registeredDocument1 = map.RegisterDocument(new Secret(id1));
            var registeredDocument2 = map.RegisterDocument(new Secret(id2));
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
        public void ShouldReturnChangedDocuments()
        {
            // Arrange
            var id1 = KnownId + 1;
            var id2 = KnownId + 2;
            var id3 = KnownId + 3;
            var id4 = KnownId + 4;

            map.RegisterDocument(new Secret(id1));
            var registeredDocument2 = map.RegisterDocument(new Secret(id2));
            map.RegisterDocument(new Secret(id3));
            var registeredDocument4 = map.RegisterDocument(new Secret(id4));

            // Act
            registeredDocument2.Name += " (Modified 2)";
            registeredDocument4.Name += " (Modified 4)";

            var changedDocuments = map.GetChangedDocuments();

            // Assert
            Assert.Equal(2, changedDocuments.Count);

            var modifiedDocument2 = Assert.Single(changedDocuments.Where(d => d.Id == id2));
            Assert.Same(registeredDocument2, modifiedDocument2);

            var modifiedDocument4 = Assert.Single(changedDocuments.Where(d => d.Id == id4));
            Assert.Same(registeredDocument4, modifiedDocument4);
        }

        private sealed class Secret : IDocument
        {
            public Secret(string id)
            {
                Id = id;
                Name = Id;
            }

            public string? Id { get; }
            public string? Name { get; set; }

            public int GetContentChecksum() => HashCode.Combine(Id, Name);
        }
    }
}
