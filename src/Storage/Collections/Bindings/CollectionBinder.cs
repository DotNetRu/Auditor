using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.FileSystem;

namespace DotNetRu.Auditor.Storage.Collections.Bindings
{
    internal class CollectionBinder
    {
        public delegate Matcher MatcherFactory(IDirectory collectionDirectory);

        private readonly MatcherFactory createMatcher;

        public CollectionBinder(MatcherFactory createMatcher)
        {
            this.createMatcher = createMatcher;
        }

        public async IAsyncEnumerable<Collection> ScanAsync(IDirectory databaseDirectory)
        {
            await foreach (var collectionDirectory in databaseDirectory.EnumerateDirectoriesAsync().ConfigureAwait(false))
            {
                var collection = await BindCollectionAsync(collectionDirectory).ConfigureAwait(false);
                if (collection != null)
                {
                    yield return collection;
                }
            }
        }

        private async Task<Collection?> BindCollectionAsync(IDirectory collectionDirectory)
        {
            var matcher = createMatcher(collectionDirectory);

            await foreach (var directory in collectionDirectory.EnumerateDirectoriesAsync().ConfigureAwait(false))
            {
                await matcher.AcceptAsync(directory).ConfigureAwait(false);
            }

            await foreach (var file in collectionDirectory.EnumerateFilesAsync().ConfigureAwait(false))
            {
                await matcher.AcceptAsync(file).ConfigureAwait(false);
            }

            var collection = matcher.Match();
            return collection;
        }
    }
}
