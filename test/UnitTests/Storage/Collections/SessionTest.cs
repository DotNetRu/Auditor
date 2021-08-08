using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Data.Model;
using DotNetRu.Auditor.Storage;
using DotNetRu.Auditor.Storage.Collections;
using Moq;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Storage.Collections
{
    // TDO: Add integration tests
    public sealed class SessionTest
    {
        private const string SomeId = nameof(SomeId);
        private static readonly SessionOptions DefaultOptions = new();
        private static readonly IDocumentSerializerFactory NoFactory = new Mock<IDocumentSerializerFactory>(MockBehavior.Strict).Object;

        [Fact]
        public async Task ShouldLoadNullWhenCollectionNotFound()
        {
            // Arrange
            var session = CreateEmptySession();

            // Act
            var secret = await session.LoadAsync<Community>(SomeId).ConfigureAwait(false);

            // Assert
            Assert.Null(secret);
        }

        [Fact]
        public async Task ShouldLoadEmptyDictionaryWhenCollectionNotFound()
        {
            // Arrange
            var session = CreateEmptySession();

            // Act
            var secrets = await session.LoadAsync<Community>(new[] { SomeId }).ConfigureAwait(false);

            // Assert
            Assert.Empty(secrets);
        }

        [Fact]
        public async Task ShouldQueryEmptyWhenCollectionNotFound()
        {
            // Arrange
            var session = CreateEmptySession();

            // Act
            var secrets = await session.QueryAsync<Community>().ToListAsync().ConfigureAwait(false);

            // Assert
            Assert.Empty(secrets);
        }

        private static Session CreateEmptySession()
        {
            return new(DefaultOptions, NoCollection);
        }

        private static bool NoCollection(Type collectionType, [NotNullWhen(true)] out IDocumentCollection? collection)
        {
            collection = default;
            return false;
        }
    }
}
