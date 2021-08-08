using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.FileSystem;

namespace DotNetRu.Auditor.Storage.Collections.Bindings
{
    internal class CollectionBinder
    {
        private readonly Func<Matcher> createMatcher;

        public CollectionBinder(Func<Matcher> createMatcher)
        {
            this.createMatcher = createMatcher;
        }

        public async IAsyncEnumerable<IDocumentCollection> ScanAsync(IDirectory databaseDirectory)
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

        private async Task<IDocumentCollection?> BindCollectionAsync(IDirectory collectionDirectory)
        {
            var matcher = createMatcher();

            await foreach (var directory in collectionDirectory.EnumerateDirectoriesAsync().ConfigureAwait(false))
            {
                await matcher.AcceptAsync(directory).ConfigureAwait(false);
            }

            await foreach (var file in collectionDirectory.EnumerateFilesAsync().ConfigureAwait(false))
            {
                await matcher.AcceptAsync(file).ConfigureAwait(false);
            }

            var collection = matcher.Match(collectionDirectory);
            return collection;
        }
    }
}
