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
            where T : IRecord;

        protected readonly IDirectory Directory;

        protected Collection(IDirectory directory)
        {
            Directory = directory;
        }

        public string Name => Directory.Name;

        public async Task<T?> LoadAsync<T>(string id, IndexDeserializer<T> deserializer)
            where T : IRecord
        {
            var indexFile = GetIndexFileAsync(id);
            var exists = await indexFile.ExistsAsync().ConfigureAwait(false);
            if (!exists)
            {
                return default;
            }

            var record = await DeserializeIndex(indexFile, deserializer).ConfigureAwait(false);
            return record;
        }

        public async IAsyncEnumerable<T> QueryAsync<T>(IndexDeserializer<T> deserializer)
            where T : IRecord
        {
            await foreach (var indexFile in EnumerateIndexFilesAsync())
            {
                var record = await DeserializeIndex(indexFile, deserializer).ConfigureAwait(false);

                if (record != null)
                {
                    yield return record;
                }
            }
        }

        protected abstract IFile GetIndexFileAsync(string id);

        protected abstract IAsyncEnumerable<IFile> EnumerateIndexFilesAsync();

        private static async Task<T?> DeserializeIndex<T>(IFile indexFile, IndexDeserializer<T> deserializer)
            where T : IRecord
        {
            await using var indexStream = await indexFile.OpenForReadAsync().ConfigureAwait(false);
            var record = await deserializer(indexStream).ConfigureAwait(false);
            return record;
        }
    }
}
