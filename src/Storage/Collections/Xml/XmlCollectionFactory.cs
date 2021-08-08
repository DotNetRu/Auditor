using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Data.Description;
using DotNetRu.Auditor.Storage.FileSystem;

namespace DotNetRu.Auditor.Storage.Collections.Xml
{
    internal sealed class XmlCollectionFactory : IXmlCollectionFactory
    {
        private readonly ModelRegistry registry;
        private readonly IDocumentSerializerFactory serializerFactory;

        public XmlCollectionFactory(ModelRegistry registry, IDocumentSerializerFactory serializerFactory)
        {
            this.registry = registry;
            this.serializerFactory = serializerFactory;
        }

        public static string ToCollectionName(string groupName) => groupName.ToLowerInvariant();

        public IDocumentCollection? Create(CollectionStructure structure, IDirectory collectionDirectory)
        {
            var collectionName = collectionDirectory.Name;
            if (!TryGetModel(collectionName, out var model))
            {
                return default;
            }

            var factory = CreateFactory(model.ModelType);
            return factory.CreateCollection(structure, collectionDirectory, serializerFactory);
        }

        private bool TryGetModel(string collectionName, [NotNullWhen(true)] out ModelDefinition? model)
        {
            model = registry.Models.FirstOrDefault(m => ToCollectionName(m.GroupName) == collectionName);
            return model != null;
        }

        private static CollectionFactory CreateFactory(Type documentType)
        {
            var genericFactoryType = typeof(CollectionFactory<>);
            Type[] documentTypes = { documentType };

            Type factoryType = genericFactoryType.MakeGenericType(documentTypes);

            var factory = Activator.CreateInstance(factoryType) as CollectionFactory;
            return factory ?? throw new InvalidOperationException($"Can't create {nameof(CollectionFactory)} for {documentType.Name}");
        }

        private abstract class CollectionFactory
        {
            public abstract IDocumentCollection CreateCollection(
                CollectionStructure structure,
                IDirectory collectionDirectory,
                IDocumentSerializerFactory serializerFactory);
        }

        private sealed class CollectionFactory<T> : CollectionFactory
            where T : IDocument
        {
            public override IDocumentCollection CreateCollection(
                CollectionStructure structure,
                IDirectory collectionDirectory,
                IDocumentSerializerFactory serializerFactory)
            {
                var serializer = serializerFactory.Create<T>();

                return structure switch
                {
                    CollectionStructure.File => new XmlFileCollection<T>(collectionDirectory, serializer),
                    CollectionStructure.Directory => new XmlDirectoryCollection<T>(collectionDirectory, serializer),
                    _ => throw new ArgumentOutOfRangeException(nameof(structure), structure, null)
                };
            }
        }
    }
}
