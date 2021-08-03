using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DotNetRu.Auditor.Data;

namespace DotNetRu.Auditor.Storage.Collections.Changes
{
    internal sealed class IdentityMap<T>
        where T: IDocument
    {
        private readonly ConcurrentDictionary<string, Entry> entries = new();

        public T RegisterDocument(T document)
        {
            var id = document.Id ?? throw new ArgumentException("Can't register document without identity");
            var entry = entries.GetOrAdd(id, (_, sourceDocument) => new(sourceDocument), document);
            return entry.Document;
        }

        public bool TryResolveDocument(string id, [NotNullWhen(true)] out T? document)
        {
            if (entries.TryGetValue(id, out var entry))
            {
                document = entry.Document;
                return true;
            }

            document = default;
            return false;
        }

        public IReadOnlyList<T> GetChangedDocuments()
        {
            return entries
                .Values
                .Where(entry => entry.HasChanges())
                .Select(entry => entry.Document)
                .ToList();
        }

        private sealed class Entry
        {
            private readonly int originChecksum;

            public Entry(T document)
            {
                Document = document;
                originChecksum = Document.GetContentChecksum();
            }

            public T Document { get; }

            public bool HasChanges()
            {
                var checksum = Document.GetContentChecksum();
                return checksum != originChecksum;
            }
        }
    }

    internal static class IdentityMapExtension
    {
        public static IReadOnlyDictionary<string, T> GetDocuments<T>(this IdentityMap<T> map, IReadOnlyList<string> ids)
            where T: IDocument
        {
            var result = new Dictionary<string, T>();
            var documents = ids
                .Select(id => map.TryResolveDocument(id, out var document) ? document : default);

            foreach (var document in documents)
            {
                if (document?.Id != null)
                {
                    result.Add(document.Id, document);
                }
            }

            return result;
        }
    }
}
