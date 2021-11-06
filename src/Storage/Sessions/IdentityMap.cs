using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        public T RegisterOrigin(T document)
        {
            var id = document.Id ?? throw new ArgumentException("Can't register document without identity");

            // TODO: what if we already have document with same key, but different content?
            var entry = entries.GetOrAdd(id, static (_, sourceDocument) => Entry.Unchanged(sourceDocument), document);
            return entry.Document;
        }

        public void RegisterNew(T document)
        {
            var id = document.Id ?? throw new ArgumentException("Can't register document without identity");

            if (!entries.TryAdd(id, Entry.Added(document)))
            {
                throw new ArgumentException("Can't register document with the same identity twice");
            }
        }

        public bool TryGet(string id, [NotNullWhen(true)] out T? document)
        {
            if (entries.TryGetValue(id, out var entry))
            {
                document = entry.Document;
                return true;
            }

            document = default;
            return false;
        }

        public IReadOnlyList<T> PopChanges()
        {
            var documents = entries
                .Values
                .Where(entry => entry.IsChangedOrAdded())
                .Select(entry => entry.Document)
                .ToList();

            entries.Clear();

            return documents;
        }

        private sealed class Entry
        {
            private readonly int originChecksum;
            private readonly bool isAdded;

            private Entry(T document, bool isAdded)
            {
                Document = document;
                this.isAdded = isAdded;
                originChecksum = Document.GetContentChecksum();
            }

            public T Document { get; }

            public static Entry Unchanged(T document) => new(document, isAdded: false);

            public static Entry Added(T document) => new(document, isAdded: true);

            public bool IsChangedOrAdded() => isAdded || HasChanges();

            private bool HasChanges()
            {
                var checksum = Document.GetContentChecksum();
                return checksum != originChecksum;
            }
        }
    }
}
