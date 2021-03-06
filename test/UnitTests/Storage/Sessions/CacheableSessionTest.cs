using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Data.Model;
using DotNetRu.Auditor.Storage.Sessions;
using DotNetRu.Auditor.UnitTests.Storage.Collections.Xml;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Storage.Sessions
{
    public sealed class CacheableSessionTest
    {
        private readonly DataSessionMock dataSession;
        private readonly DataCache dataCache;
        private readonly CacheableSession session;

        private const string Id1 = "Id-1";
        private const string Id2 = "Id-2";
        private const string Id3 = "Id-3";

        private readonly Community community1 = Mocker.Community(Id1);
        private readonly Community community2 = Mocker.Community(Id2);
        private readonly Community community3 = Mocker.Community(Id3);

        public CacheableSessionTest()
        {
            dataSession = new DataSessionMock();
            dataCache = new DataCache();
            session = new CacheableSession(dataSession, dataCache);
        }

        [Fact]
        public async Task ShouldLoadFromCacheWhenInCache()
        {
            // Arrange
            AssertCacheIsEmpty();
            AddToCache(community1);

            // Act
            var document = await session.LoadAsync<Community>(Id1).ConfigureAwait(false);

            // Assert
            Assert.NotNull(document);
            Assert.Same(community1, document);
            Assert.Empty(dataSession.WasTouched);
        }

        [Fact]
        public async Task ShouldLoadDefaultWhenDeleted()
        {
            // Arrange
            AssertCacheIsEmpty();
            dataSession.Initialize(community1);
            AddToCache(community1);

            // Act
            await session.DeleteAsync(community1).ConfigureAwait(false);

            var document = await session.LoadAsync<Community>(Id1).ConfigureAwait(false);

            // Assert
            Assert.Null(document);
            Assert.Empty(dataSession.WasTouched);
        }

        [Fact]
        public async Task ShouldLoadFromSessionWhenNoInCache()
        {
            // Arrange
            AssertCacheIsEmpty();
            dataSession.Initialize(community1);

            // Act
            var document = await session.LoadAsync<Community>(Id1).ConfigureAwait(false);

            // Assert
            Assert.NotNull(document);
            Assert.Same(community1, document);
            AssertEx.Equivalence(Id1.AsEnumerable(), dataSession.WasQueried);

            AssertInCache(community1);
        }

        [Fact]
        public async Task ShouldLoadDefaultByUnknownId()
        {
            // Arrange
            AssertCacheIsEmpty();

            // Act
            var document = await session.LoadAsync<Community>(Id1).ConfigureAwait(false);

            // Assert
            Assert.Null(document);
            Assert.Equal(1, dataSession.WasTouched.Count);
            AssertEx.Equivalence(Id1.AsEnumerable(), dataSession.WasQueried);

            AssertCacheIsEmpty();
        }

        [Fact]
        public async Task ShouldPartialLoadManyFromCache()
        {
            // Arrange
            AssertCacheIsEmpty();
            AddToCache(community1);
            AddToCache(community2);
            dataSession.Initialize(community3);

            var ids = new[] { Id1, Id2, Id3 };

            // Act
            await session.DeleteAsync(community2).ConfigureAwait(false);

            var documents = await session.LoadAsync<Community>(ids).ConfigureAwait(false);

            // Assert
            Assert.Equal(2, documents.Count);
            Assert.Same(community1, documents[Id1]);
            Assert.Same(community3, documents[Id3]);

            AssertEx.Equivalence(Id3.AsEnumerable(), dataSession.WasQueried);

            AssertInCache(community1);
            AssertInCache(community2, asDeleted: true);
            AssertInCache(community3);
        }

        [Fact]
        public async Task ShouldFullyLoadManyFromCache()
        {
            // Arrange
            AssertCacheIsEmpty();
            AddToCache(community1);
            AddToCache(community2);
            AddToCache(community3);
            dataSession.Initialize(community3);

            var ids = new[] { Id1, Id2, Id3 };

            // Act
            await session.DeleteAsync(community3).ConfigureAwait(false);

            var documents = await session.LoadAsync<Community>(ids).ConfigureAwait(false);

            // Assert
            Assert.Equal(2, documents.Count);
            Assert.Same(community1, documents[Id1]);
            Assert.Same(community2, documents[Id2]);

            Assert.Empty(dataSession.WasTouched);

            AssertInCache(community1);
            AssertInCache(community2);
            AssertInCache(community3, asDeleted: true);
        }

        [Fact]
        public async Task ShouldQueryFromSessionWhenNoInCache()
        {
            // Arrange
            AssertCacheIsEmpty();
            dataSession.Initialize(community1, community2, community3);

            // Act
            await session.DeleteAsync(community3).ConfigureAwait(false);

            var documents = await session.QueryAsync<Community>().ToListAsync().ConfigureAwait(false);

            // Assert
            Assert.Equal(2, documents.Count);
            Assert.Same(community1, Assert.Single(documents, document => document.Id == Id1));
            Assert.Same(community2, Assert.Single(documents, document => document.Id == Id2));

            AssertEx.Equivalence(new[] { Id1, Id2, Id3 }, dataSession.WasQueried);

            AssertInCache(community1);
            AssertInCache(community2);
            AssertInCache(community3, asDeleted: true);
        }

        [Fact]
        public async Task ShouldDelayAddToSession()
        {
            // Arrange

            // Act
            await session.AddAsync(community1).ConfigureAwait(false);

            // Assert
            Assert.Empty(dataSession.WasTouched);

            AssertInCache(community1);
        }

        [Fact]
        public async Task ShouldDelayDeleteFromSession()
        {
            // Arrange
            AssertCacheIsEmpty();
            dataSession.Initialize(community1);
            AddToCache(community1);

            // Act
            await session.DeleteAsync(community1).ConfigureAwait(false);

            // Assert
            Assert.Empty(dataSession.WasTouched);

            AssertInCache(community1, asDeleted: true);
        }

        [Fact]
        public async Task ShouldWriteWhenSaveChanges()
        {
            // Arrange
            const string mutableId = "id-mutable";
            dataSession.Initialize(community1, Mocker.Community(mutableId));

            var maybeCommunity = await session.LoadAsync<Community>(mutableId).ConfigureAwait(false);
            var mutableCommunity = AssertEx.NotNull(maybeCommunity);

            // Act
            mutableCommunity.Name += " (changed)";

            await session.AddAsync(community2).ConfigureAwait(false);

            await session.SaveChangesAsync().ConfigureAwait(false);

            // Assert
            Assert.Equal(2, dataSession.WasTouched.Count);
            AssertEx.Equivalence(new[] { mutableId, Id2 }, dataSession.WasWritten);
        }

        [Fact]
        public async Task ShouldDeleteWhenSaveChanges()
        {
            // Arrange
            dataSession.Initialize(community1, community2, community3);
            AddToCache(community1);

            // Act
            await session.DeleteAsync(community1).ConfigureAwait(false);
            await session.DeleteAsync(community3).ConfigureAwait(false);

            await session.SaveChangesAsync().ConfigureAwait(false);

            // Assert
            Assert.Equal(2, dataSession.WasTouched.Count);
            AssertEx.Equivalence(new[] { Id1, Id3 }, dataSession.WasDeleted);
        }

        [Fact]
        public async Task ShouldNotWriteTwice()
        {
            // Arrange
            const string mutableId = "id-mutable";
            dataSession.Initialize(community1, Mocker.Community(mutableId));

            var maybeCommunity = await session.LoadAsync<Community>(mutableId).ConfigureAwait(false);
            var mutableCommunity = AssertEx.NotNull(maybeCommunity);

            mutableCommunity.Name += " (changed)";
            await session.AddAsync(community2).ConfigureAwait(false);
            await session.DeleteAsync(community3).ConfigureAwait(false);
            await session.SaveChangesAsync().ConfigureAwait(false);

            dataSession.ResetCounters();

            // Act
            await session.SaveChangesAsync().ConfigureAwait(false);

            // Assert
            Assert.Empty(dataSession.WasTouched);
            AssertCacheIsEmpty();
        }

        private void AssertCacheIsEmpty()
        {
            var amount = dataCache.GetAllMaps().Sum(map => map.Count);
            Assert.Equal(0, amount);
        }

        private void AssertInCache<T>(T document, bool asDeleted = false)
            where T : class, IDocument
        {
            var map = dataCache.GetMap<T>();
            var id = document.Id ?? throw NoId();
            var (cache, isDeleted) = map.Resolve(id);

            if (asDeleted)
            {
                Assert.Null(cache);
                Assert.True(isDeleted);
            }
            else
            {
                Assert.NotNull(cache);
                Assert.Same(document, cache);
                Assert.False(isDeleted);
            }
        }

        private void AddToCache<T>(params T[] documents)
            where T : IDocument
        {
            var map = dataCache.GetMap<T>();

            foreach (var document in documents)
            {
                map.RegisterOrigin(document);
            }
        }

        private static InvalidOperationException NoId() => new("Entity identity can't be null");

        private sealed class DataSessionMock : IDataSession
        {
            private readonly ConcurrentDictionary<string, IDocument> entities = new();
            private readonly HashSet<string> wasQueried = new();
            private readonly HashSet<string> wasWritten = new();
            private readonly HashSet<string> wasDeleted = new();

            public IReadOnlyList<string> WasTouched => Enumerable.Empty<string>()
                .Union(wasQueried)
                .Union(wasWritten)
                .Union(wasDeleted)
                .Distinct()
                .ToList();

            public IReadOnlyList<string> WasQueried => wasQueried.ToList();

            public IReadOnlyList<string> WasWritten => wasWritten.ToList();

            public IReadOnlyList<string> WasDeleted => wasDeleted.ToList();

            public void Initialize<T>(params T[] documents)
                where T : IDocument
            {
                foreach (var document in documents)
                {
                    var id = document.Id ?? throw NoId();
                    entities.TryAdd(id, document);
                }
            }

            public void ResetCounters()
            {
                wasQueried.Clear();
                wasWritten.Clear();
                wasDeleted.Clear();
            }

            public Task<T?> LoadAsync<T>(string id)
                where T : IDocument
            {
                wasQueried.Add(id);
                var document = entities.TryGetValue(id, out var entity) ? (T)entity : default;
                return Task.FromResult(document);
            }

            public Task<IReadOnlyDictionary<string, T>> LoadAsync<T>(IReadOnlyList<string> ids)
                where T : IDocument
            {
                wasQueried.UnionWith(ids);
                var documents = new Dictionary<string, T>();

                foreach (var id in ids)
                {
                    if (entities.TryGetValue(id, out var entity))
                    {
                        documents.Add(id, (T)entity);
                    }
                }

                return Task.FromResult<IReadOnlyDictionary<string, T>>(documents);
            }

            public IAsyncEnumerable<T> QueryAsync<T>()
                where T : IDocument
            {
                var documents = entities.Values.OfType<T>().ToList();
                wasQueried.UnionWith(documents.Select(document => document.Id ?? throw NoId()));
                return documents.ToAsyncEnumerable();
            }

            public Task AddAsync<T>(T document)
                where T : IDocument =>
                WriteAsync(document.AsEnumerable());

            public Task DeleteAsync<T>(T document)
                where T : IDocument =>
                DeleteAsync(document.AsEnumerable());

            public Task WriteAsync<T>(IReadOnlyList<T> documents)
                where T : IDocument
            {
                foreach (var document in documents)
                {
                    var id = document.Id ?? throw NoId();
                    wasWritten.Add(id);
                    entities.TryAdd(id, document);
                }

                return Task.CompletedTask;
            }

            public Task DeleteAsync<T>(IReadOnlyList<T> documents)
                where T : IDocument
            {
                foreach (var document in documents)
                {
                    var id = document.Id ?? throw NoId();
                    wasDeleted.Add(id);
                    entities.TryRemove(id, out _);
                }

                return Task.CompletedTask;
            }
        }
    }
}
