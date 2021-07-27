using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.FileSystem;

namespace DotNetRu.Auditor.Storage.Collections.Bindings
{
    internal abstract class Matcher
    {
        public string? ErrorMessage { get; protected set; }

        public abstract Task AcceptAsync(IFile file);

        public abstract Task AcceptAsync(IDirectory directory);

        public abstract Collection? Match();
    }
}