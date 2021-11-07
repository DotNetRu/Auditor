using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data.Model;
using DotNetRu.Auditor.Storage;
using DotNetRu.Auditor.Storage.Collections;
using DotNetRu.Auditor.Storage.Sessions;
using DotNetRu.Auditor.UnitTests.Storage.Collections.Xml;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Storage.Sessions
{
    public sealed class EmptyDataSessionTest
    {
        private const string SomeId = nameof(SomeId);

        [Fact]
        public async Task ShouldLoadNullWhenCollectionNotFound()
        {
            // Arrange
            var session = CreateEmptySession();

            // Act
            var community = await session.LoadAsync<Community>(SomeId).ConfigureAwait(false);

            // Assert
            Assert.Null(community);
        }

        [Fact]
        public async Task ShouldLoadEmptyDictionaryWhenCollectionNotFound()
        {
            // Arrange
            var session = CreateEmptySession();

            // Act
            var communities = await session.LoadAsync<Community>(new[] { SomeId }).ConfigureAwait(false);

            // Assert
            Assert.Empty(communities);
        }

        [Fact]
        public async Task ShouldQueryEmptyWhenCollectionNotFound()
        {
            // Arrange
            var session = CreateEmptySession();

            // Act
            var communities = await session.QueryAsync<Community>().ToListAsync().ConfigureAwait(false);

            // Assert
            Assert.Empty(communities);
        }

        [Fact]
        public async Task ShouldRaiseErrorWhenWriteToUnknownCollection()
        {
            // Arrange
            var session = CreateEmptySession();
            var community = Mocker.Community(SomeId);

            // Act
            Task WriteToUnknownCollection() => session.AddAsync(community);

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(WriteToUnknownCollection).ConfigureAwait(false);
        }

        [Fact]
        public async Task ShouldRaiseErrorWhenDeleteFromUnknownCollection()
        {
            // Arrange
            var session = CreateEmptySession();
            var community = Mocker.Community(SomeId);

            // Act
            Task DeleteFromUnknownCollection() => session.DeleteAsync(community);

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(DeleteFromUnknownCollection).ConfigureAwait(false);
        }

        private static ISession CreateEmptySession()
        {
            return new DataSession(NoCollection);
        }

        private static bool NoCollection(Type collectionType, [NotNullWhen(true)] out IDocumentCollection? collection)
        {
            collection = default;
            return false;
        }
    }
}
