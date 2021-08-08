using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetRu.Auditor.Data.Description
{
    public sealed class DocumentSerializerFactory : IDocumentSerializerFactory
    {
        private readonly IReadOnlyDictionary<Type, IDocumentSerializer> serializers;

        public DocumentSerializerFactory(ModelRegistry registry)
        {
            serializers = registry
                .Models
                .ToDictionary(model => model.ModelType, model => model.Serializer);
        }

        public IDocumentSerializer<T> Create<T>()
            where T : IDocument
        {
            if (serializers.TryGetValue(typeof(T), out var serializer))
            {
                return (IDocumentSerializer<T>)serializer;
            }

            throw new InvalidOperationException($"Can't find serializer for {typeof(T).Name}");
        }
    }
}
