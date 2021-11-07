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
            var indexFile = GetIndexFile(id);
            var exists = await indexFile.ExistsAsync().ConfigureAwait(false);
            if (!exists)
            {
                return default;
            }

            var document = await DeserializeIndexAsync(indexFile).ConfigureAwait(false);
            return document;
        }

        public async IAsyncEnumerable<T> QueryAsync()
        {
            await foreach (var indexFile in EnumerateIndexFilesAsync().ConfigureAwait(false))
            {
                var document = await DeserializeIndexAsync(indexFile).ConfigureAwait(false);

                if (document != null)
                {
                    yield return document;
                }
            }
        }

        public Task WriteAsync(T document)
        {
            var id = document.Id ?? throw new ArgumentException("Can't write document without identity");
            var indexFile = GetIndexFile(id);

            return SerializeIndexAsync(indexFile, document);
        }

        public abstract Task<bool> DeleteAsync(string id);

        protected abstract IFile GetIndexFile(string id);

        protected abstract IAsyncEnumerable<IFile> EnumerateIndexFilesAsync();

        private async Task<T?> DeserializeIndexAsync(IFile indexFile)
        {
            var indexStream = await indexFile.OpenForReadAsync().ConfigureAwait(false);
            await using (indexStream.ConfigureAwait(false))
            {
                var document = await serializer.DeserializeAsync(indexStream).ConfigureAwait(false);
                return document;
            }
        }

        private async Task SerializeIndexAsync(IFile indexFile, T document)
        {
            var writableFile = await indexFile.RequestWriteAccessAsync().ConfigureAwait(false);
            if (writableFile == null)
            {
                throw new InvalidOperationException($"Can't write to «{indexFile.FullName}» file");
            }

            var indexStream = await writableFile.OpenForWriteAsync().ConfigureAwait(false);
            await using (indexStream.ConfigureAwait(false))
            {
                await serializer.SerializeAsync(indexStream, document).ConfigureAwait(false);
            }
        }
    }
}
