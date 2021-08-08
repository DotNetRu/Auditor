using System;

namespace DotNetRu.Auditor.Data.Description
{
    public sealed class ModelDefinition
    {
        internal ModelDefinition(string name, string groupName, Type modelType, IDocumentSerializer serializer)
        {
            Name = name;
            GroupName = groupName;
            ModelType = modelType;
            Serializer = serializer;
        }

        public string Name { get; }

        public string GroupName { get; }

        public Type ModelType { get; }

        internal IDocumentSerializer Serializer { get; }
    }
}
