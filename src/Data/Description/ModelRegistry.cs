using System.Collections.Generic;

namespace DotNetRu.Auditor.Data.Description
{
    public sealed class ModelRegistry
    {
        public static ModelRegistry Instance => new(ModelFactory.CreateModels());

        private ModelRegistry(IReadOnlyList<ModelDefinition> models)
        {
            Models = models;
        }

        public IReadOnlyList<ModelDefinition> Models { get;}
    }
}
