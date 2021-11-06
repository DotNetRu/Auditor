using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.Collections;
using DotNetRu.Auditor.Storage.Collections.Bindings;
using DotNetRu.Auditor.Storage.FileSystem;
using DotNetRu.Auditor.Storage.Sessions;

namespace DotNetRu.Auditor.Storage
{
    public static class AuditStore
    {
        private static readonly Lazy<ServiceContainer> Container = new(ServiceContainer.Build);

        public static async Task<IStore> OpenAsync(IDirectory databaseDirectory, StoreOptions? storeOptions = null)
        {
            storeOptions ??= new StoreOptions();
            var collections = await ScanCollectionsAsync(databaseDirectory).ConfigureAwait(false);
            var storage = new Store(storeOptions, collections);
            return storage;
        }

        private static async Task<IReadOnlyList<IDocumentCollection>> ScanCollectionsAsync(IDirectory databaseDirectory)
        {
            var binder = new CollectionBinder(CreateMatcher);
            var collections = new List<IDocumentCollection>();
            await foreach (var collection in binder.ScanAsync(databaseDirectory).ConfigureAwait(false))
            {
                collections.Add(collection);
            }

            return collections;
        }

        private static Matcher CreateMatcher()
        {
            var container = Container.Value;
            var childMatchers = container.ResolveAllMatchers();
            return new CompositeMatcher(childMatchers);
        }
    }
}
