using System.Collections.Generic;
using System.Linq;
using DotNetRu.Auditor.Data;

namespace DotNetRu.Auditor.Storage.Sessions
{
    internal static class IdentityMapExtension
    {
        public static IReadOnlyDictionary<string, T> GetDocuments<T>(this IdentityMap<T> map, IReadOnlyList<string> ids)
            where T: IDocument
        {
            var result = new Dictionary<string, T>();
            var documents = ids
                .Select(id => map.TryGet(id, out var document) ? document : default);

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
