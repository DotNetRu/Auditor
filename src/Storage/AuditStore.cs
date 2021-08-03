using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.Collections;
using DotNetRu.Auditor.Storage.Collections.Bindings;
using DotNetRu.Auditor.Storage.Collections.Xml;
using DotNetRu.Auditor.Storage.FileSystem;

namespace DotNetRu.Auditor.Storage
{
    public static class AuditStore
    {
        public static async Task<IStore> OpenAsync(IDirectory databaseDirectory, StoreOptions? storeOptions = null)
        {
            storeOptions ??= new StoreOptions();
            var storeCollections = await ScanCollectionsAsync(databaseDirectory).ConfigureAwait(false);
            var storage = new Store(storeOptions, storeCollections);
            return storage;
        }

        private static async Task<IReadOnlyList<Collection>> ScanCollectionsAsync(IDirectory databaseDirectory)
        {
            var binder = new CollectionBinder(CreateMatcher);
            var storeCollections = new List<Collection>();
            await foreach (var collection in binder.ScanAsync(databaseDirectory).ConfigureAwait(false))
            {
                storeCollections.Add(collection);
            }

            return storeCollections;
        }

        private static Matcher CreateMatcher(IDirectory collectionDirectory)
        {
            var childMatchers = new Matcher[]
            {
                new XmlFileMatcher(collectionDirectory),
                new XmlDirectoryMatcher(collectionDirectory)
            };

            return new CompositeMatcher(childMatchers);
        }
    }
}
