using System.Collections.Generic;
using DotNetRu.Auditor.Data;

namespace DotNetRu.Auditor.Storage.Sessions
{
    internal interface IDataCache
    {
        IdentityMap<T> GetMap<T>()
            where T : IDocument;

        IReadOnlyList<IIdentityMap> GetAllMaps();
    }
}
