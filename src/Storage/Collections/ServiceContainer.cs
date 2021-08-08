using System.Collections.Generic;
using DotNetRu.Auditor.Data.Description;
using DotNetRu.Auditor.Storage.Collections.Bindings;
using DotNetRu.Auditor.Storage.Collections.Xml;

namespace DotNetRu.Auditor.Storage.Collections
{
    internal sealed class ServiceContainer
    {
        private readonly IXmlCollectionFactory collectionFactory;

        private ServiceContainer(IXmlCollectionFactory collectionFactory)
        {
            this.collectionFactory = collectionFactory;
        }

        public static ServiceContainer Build()
        {
            var registry = ModelRegistry.Instance;
            var serializerFactory = new DocumentSerializerFactory(registry);
            var collectionFactory = new XmlCollectionFactory(registry, serializerFactory);

            return new ServiceContainer(collectionFactory);
        }

        public IReadOnlyList<Matcher> ResolveAllMatchers() => new Matcher[]
        {
            new XmlFileMatcher(collectionFactory),
            new XmlDirectoryMatcher(collectionFactory)
        };
    }
}
