using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DotNetRu.Auditor.Data;

namespace DotNetRu.Auditor.Storage.Sessions
{
    internal interface IIdentityMap
    {
        int Count { get; }
    }

    internal sealed class IdentityMap<T> : IIdentityMap
        where T: IDocument
    {
        private readonly ConcurrentDictionary<string, Entry> entries = new();

        public int Count => entries.Count;

        public DocumentStatus RegisterOrigin(T document)
        {
            var id = document.Id ?? throw new ArgumentException("Can't register document without identity");

            // TODO: what if we already have document with same key, but different content?
            var entry = entries.GetOrAdd(id, static (_, sourceDocument) => Entry.Origin(sourceDocument), document);

            return DocumentStatusFrom(entry);
        }

        public void RegisterNew(T document)
        {
            var id = document.Id ?? throw new ArgumentException("Can't register document without identity");

            if (!entries.TryAdd(id, Entry.Added(document)))
            {
                throw new ArgumentException("Can't register document with the same identity twice");
            }
        }

        public void RegisterDeleted(T document)
        {
            var id = document.Id ?? throw new ArgumentException("Can't register document without identity");

            var entry = entries.GetOrAdd(id, static (_, sourceDocument) => Entry.Deleted(sourceDocument), document);
            entry.MarkAsDeleted();
        }

        public DocumentStatus Resolve(string id)
        {
            if (entries.TryGetValue(id, out var entry))
            {
                return DocumentStatusFrom(entry);
            }

            return DocumentStatus.Unknown();
        }

        public (IReadOnlyDictionary<string, T> Cache, IReadOnlyDictionary<string, T> Deleted) Resolve(IReadOnlyList<string> ids)
        {
            var cache = new Dictionary<string, T>(ids.Count);
            var deleted = new Dictionary<string, T>();

            foreach (var id in ids)
            {
                if (entries.TryGetValue(id, out var entry))
                {
                    if (entry.IsDeleted())
                    {
                        deleted.Add(id, entry.Document);
                    }
                    else
                    {
                        cache.Add(id, entry.Document);
                    }
                }
            }

            return (cache, deleted);
        }

        public (IReadOnlyList<T> writeList, IReadOnlyList<T> deleteList) PopChanges()
        {
            // TDO: If ID has been changed we should remove the old document and add the current one as new
            if (entries.IsEmpty)
            {
                return (Array.Empty<T>(), Array.Empty<T>());
            }

            var writeList = new List<T>(entries.Count);
            var deleteList = new List<T>();

            foreach (var entry in entries.Values)
            {
                if (entry.IsDeleted())
                {
                    deleteList.Add(entry.Document);
                }
                else if (entry.IsChangedOrAdded())
                {
                    writeList.Add(entry.Document);
                }
            }

            entries.Clear();

            return (writeList, deleteList);
        }

        private static DocumentStatus DocumentStatusFrom(Entry entry) =>
            entry.IsDeleted() ? DocumentStatus.Deleted() : DocumentStatus.Cached(entry.Document);

        public record DocumentStatus(T? Cache, bool IsDeleted)
        {
            public static DocumentStatus Cached(T document) => new(Cache: document, IsDeleted: false);
            public static DocumentStatus Deleted() => new(Cache: default, IsDeleted: true);
            public static DocumentStatus Unknown() => new(Cache: default, IsDeleted: false);
        }

        private sealed class Entry
        {
            private readonly int originChecksum;
            private EntryStatus status;

            private Entry(T document, EntryStatus status)
            {
                Document = document;
                this.status = status;
                originChecksum = Document.GetContentChecksum();
            }

            public T Document { get; }

            public static Entry Origin(T document) => new(document, EntryStatus.Origin);

            public static Entry Added(T document) => new(document, EntryStatus.Added);

            public static Entry Deleted(T document) => new(document, EntryStatus.Deleted);

            public void MarkAsDeleted()
            {
                status = EntryStatus.Deleted;
            }

            public bool IsDeleted() => status == EntryStatus.Deleted;

            public bool IsChangedOrAdded()
            {
                return
                    status == EntryStatus.Added ||
                    (status == EntryStatus.Origin && HasChanges());
            }

            private bool HasChanges()
            {
                var checksum = Document.GetContentChecksum();
                return checksum != originChecksum;
            }
        }

        private enum EntryStatus
        {
            Origin,
            Added,
            Deleted
        }
    }
}
