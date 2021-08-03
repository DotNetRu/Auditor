using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Storage.FileSystem;

namespace DotNetRu.Auditor.Storage.Collections
{
    internal abstract class Collection
    {
        public delegate Task<T?> IndexDeserializer<T>(Stream stream)
            where T : IDocument;

        protected readonly IDirectory Directory;

        protected Collection(IDirectory directory)
        {
            Directory = directory;
        }

        public string Name => Directory.Name;

        public async Task<T?> LoadAsync<T>(string id, IndexDeserializer<T> deserializer)
            where T : IDocument
        {
            var indexFile = GetIndexFileAsync(id);
            var exists = await indexFile.ExistsAsync().ConfigureAwait(false);
            if (!exists)
            {
                return default;
            }

            var document = await DeserializeIndex(indexFile, deserializer).ConfigureAwait(false);
            return document;
        }

        public async IAsyncEnumerable<T> QueryAsync<T>(IndexDeserializer<T> deserializer)
            where T : IDocument
        {
            await foreach (var indexFile in EnumerateIndexFilesAsync().ConfigureAwait(false))
            {
                var document = await DeserializeIndex(indexFile, deserializer).ConfigureAwait(false);

                if (document != null)
                {
                    yield return document;
                }
            }
        }

        protected abstract IFile GetIndexFileAsync(string id);

        protected abstract IAsyncEnumerable<IFile> EnumerateIndexFilesAsync();

        private static async Task<T?> DeserializeIndex<T>(IFile indexFile, IndexDeserializer<T> deserializer)
            where T : IDocument
        {
            await using var indexStream = await indexFile.OpenForReadAsync().ConfigureAwait(false);
            var document = await deserializer(indexStream).ConfigureAwait(false);
            return document;
        }
    }
}
