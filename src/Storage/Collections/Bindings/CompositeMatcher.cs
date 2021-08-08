using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Storage.FileSystem;

namespace DotNetRu.Auditor.Storage.Collections.Bindings
{
    internal sealed class CompositeMatcher : Matcher
    {
        private readonly IReadOnlyList<Matcher> matchers;

        public CompositeMatcher(IReadOnlyList<Matcher> matchers)
        {
            this.matchers = matchers;
        }

        public override Task AcceptAsync(IFile file) => matchers
            .Select(matcher => matcher.AcceptAsync(file))
            .WhenAll();

        public override Task AcceptAsync(IDirectory directory) => matchers
            .Select(matcher => matcher.AcceptAsync(directory))
            .WhenAll();

        public override IDocumentCollection? Match(IDirectory collectionDirectory)
        {
            var collections = matchers
                .Select(matcher => matcher.Match(collectionDirectory))
                .Where(match => match != default)
                .ToList();

            switch (collections.Count)
            {
                case 1:
                    return collections[0];
                case 0:
                    ErrorMessage = "No matches found";
                    return null;
                default:
                    ErrorMessage = "More than one match found";
                    return null;
            }
        }
    }
}
