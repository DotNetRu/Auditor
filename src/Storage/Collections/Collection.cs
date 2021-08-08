using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Storage.FileSystem;

namespace DotNetRu.Auditor.Storage.Collections
{
    internal abstract class Collection<T> : IDocumentCollection
        where T : IDocument
    {
        protected readonly IDirectory Directory;
        private readonly IDocumentSerializer<T> serializer;

        protected Collection(IDirectory directory, IDocumentSerializer<T> serializer)
        {
            Directory = directory;
            this.serializer = serializer;
        }

        public string Name => Directory.Name;

        public Type CollectionType => typeof(T);

        public async Task<T?> LoadAsync(string id)
        {
            var indexFile = GetIndexFileAsync(id);
            var exists = await indexFile.ExistsAsync().ConfigureAwait(false);
            if (!exists)
            {
                return default;
            }

            var document = await DeserializeIndex(indexFile).ConfigureAwait(false);
            return document;
        }

        public async IAsyncEnumerable<T> QueryAsync()
        {
            await foreach (var indexFile in EnumerateIndexFilesAsync().ConfigureAwait(false))
            {
                var document = await DeserializeIndex(indexFile).ConfigureAwait(false);

                if (document != null)
                {
                    yield return document;
                }
            }
        }

        protected abstract IFile GetIndexFileAsync(string id);

        protected abstract IAsyncEnumerable<IFile> EnumerateIndexFilesAsync();

        private async Task<T?> DeserializeIndex(IFile indexFile)
        {
            await using var indexStream = await indexFile.OpenForReadAsync().ConfigureAwait(false);
            var document = await serializer.DeserializeAsync(indexStream).ConfigureAwait(false);
            return document;
        }
    }
}
