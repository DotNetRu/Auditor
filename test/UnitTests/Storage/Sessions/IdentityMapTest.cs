using System;
using System.Collections.Generic;
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
            var (cache1, isDeleted1) = map.RegisterOrigin(document);
            var (cache2, isDeleted2) = map.RegisterOrigin(document);

            // Assert
            Assert.NotNull(cache1);
            Assert.NotNull(cache2);
            Assert.Same(cache1, cache2);
            Assert.False(isDeleted1);
            Assert.False(isDeleted2);
        }

        [Fact]
        public void ShouldReturnDeletedWhenRegisterDeleted()
        {
            // Arrange
            var origin = Mocker.Community(KnownId);
            var document = AssertEx.NotNull(map.RegisterOrigin(origin).Cache);

            // Act
            map.RegisterDeleted(document);
            var (cache, isDeleted) = map.RegisterOrigin(origin);

            // Assert
            Assert.Null(cache);
            Assert.True(isDeleted);
        }

        [Fact]
        public void ShouldResolveTheSameInstanceWhenNewRegistered()
        {
            // Arrange
            var document = Mocker.Community(KnownId);
            map.RegisterNew(document);

            // Act
            var (resolvedDocument, isDeleted) = map.Resolve(KnownId);

            // Assert
            Assert.NotNull(resolvedDocument);
            Assert.Same(document, resolvedDocument);
            Assert.False(isDeleted);
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
        public void ShouldDelete()
        {
            // Arrange
            var origin = Mocker.Community(KnownId);

            // Act
            map.RegisterDeleted(origin);
            var (resolvedDocument, isDeleted) = map.Resolve(KnownId);

            // Assert
            Assert.Null(resolvedDocument);
            Assert.True(isDeleted);
        }

        [Fact]
        public void ShouldResolveTheSameInstanceWhenRegistered()
        {
            // Arrange
            var document = Mocker.Community(KnownId);
            var registeredDocument = map.RegisterOrigin(document).Cache;

            // Act
            var (resolvedDocument, isDeleted) = map.Resolve(KnownId);

            // Assert
            Assert.NotNull(resolvedDocument);
            Assert.Same(registeredDocument, resolvedDocument);
            Assert.False(isDeleted);
        }

        [Fact]
        public void ShouldResolveDeleted()
        {
            // Arrange
            var origin = Mocker.Community(KnownId);
            var document = AssertEx.NotNull(map.RegisterOrigin(origin).Cache);

            // Act
            map.RegisterDeleted(document);
            var (resolvedDocument, isDeleted) = map.Resolve(KnownId);

            // Assert
            Assert.Null(resolvedDocument);
            Assert.True(isDeleted);
        }

        [Fact]
        public void ShouldResolveUnknownWhenNotRegistered()
        {
            // Arrange
            const string unknownId = nameof(ShouldResolveUnknownWhenNotRegistered);

            // Act
            var (cache, isDeleted) = map.Resolve(unknownId);

            // Assert
            Assert.Null(cache);
            Assert.False(isDeleted);
        }

        [Fact]
        public void ShouldResolveMany()
        {
            // Arrange
            var id1 = KnownId + 1;
            var id2 = KnownId + 2;
            var id3 = KnownId + 3;
            var id4 = KnownId + 4;
            var registeredDocument1 = map.RegisterOrigin(Mocker.Community(id1)).Cache;
            var registeredDocument2 = Mocker.Community(id2);
            map.RegisterNew(registeredDocument2);
            var registeredDocument4 = AssertEx.NotNull(map.RegisterOrigin(Mocker.Community(id4)).Cache);
            map.RegisterDeleted(registeredDocument4);
            var ids = new[] { id2, id1, id3, id4 };

            // Act
            var (cache, deleted) = map.Resolve(ids);

            // Assert
            Assert.Equal(2, cache.Count);
            Assert.Same(registeredDocument1, cache[id1]);
            Assert.Same(registeredDocument2, cache[id2]);

            Assert.Equal(1, deleted.Count);
            Assert.Same(registeredDocument4, deleted[id4]);

            Assert.DoesNotContain(id3, cache.Keys);
        }

        [Fact]
        public void ShouldPopEmptyWhenNothingChanged()
        {
            // Arrange

            // Act
            var (writeList, deleteList) = map.PopChanges();

            // Assert
            Assert.Empty(writeList);
            Assert.Empty(deleteList);
        }

        [Fact]
        public void ShouldPopChanges()
        {
            // Arrange
            var id1 = KnownId + 1; // Unchanged
            var id2 = KnownId + 2; // Name changed
            var id3 = KnownId + 3; // Deleted
            var id4 = KnownId + 4; // Name changed
            var id5 = KnownId + 5; // Added

            map.RegisterOrigin(Mocker.Community(id1));
            var maybeDocument2 = map.RegisterOrigin(Mocker.Community(id2));
            var registeredDocument2 = AssertEx.NotNull(maybeDocument2.Cache);
            var maybeDocument3 = map.RegisterOrigin(Mocker.Community(id3));
            var registeredDocument3 = AssertEx.NotNull(maybeDocument3.Cache);
            var maybeDocument4 = map.RegisterOrigin(Mocker.Community(id4));
            var registeredDocument4 = AssertEx.NotNull(maybeDocument4.Cache);
            var newDocument5 = Mocker.Community(id5);

            // Act
            registeredDocument2.Name += " (Modified 2)";
            map.RegisterDeleted(registeredDocument3);
            registeredDocument4.Name += " (Modified 4)";
            map.RegisterNew(newDocument5);

            var (writeList, deleteList) = map.PopChanges();

            // Assert
            Assert.Equal(3, writeList.Count);

            var modifiedDocument2 = Assert.Single(writeList.Where(d => d.Id == id2));
            Assert.Same(registeredDocument2, modifiedDocument2);

            var modifiedDocument4 = Assert.Single(writeList.Where(d => d.Id == id4));
            Assert.Same(registeredDocument4, modifiedDocument4);

            var addedDocument5 = Assert.Single(writeList.Where(d => d.Id == id5));
            Assert.Same(newDocument5, addedDocument5);

            Assert.Equal(1, deleteList.Count);
            var deletedDocument3 = Assert.Single(deleteList.Where(d => d.Id == id3));
            Assert.Same(registeredDocument3, deletedDocument3);

            Assert.Equal(0, map.Count);
        }
    }
}
