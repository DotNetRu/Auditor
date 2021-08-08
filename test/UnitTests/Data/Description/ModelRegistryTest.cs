using System;
using System.Linq;
using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Data.Description;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Data.Description
{
    public sealed class ModelRegistryTest
    {
        [Theory]
        [MemberData(nameof(ModelTypes))]
        public void ShouldKnowModel(Type modelType)
        {
            // Arrange
            var registry = ModelRegistry.Instance;

            // Act
            var model = registry.Models.FirstOrDefault(m => m.ModelType == modelType);

            // Assert
            Assert.NotNull(model);
        }

        [Fact]
        public void ShouldHaveModelTypes()
        {
            Assert.NotEmpty(ModelTypes);
        }

        public static TheoryData<Type> ModelTypes => typeof(IDocument)
            .Assembly
            .ExportedTypes
            .Where(type => type.IsPublic)
            .Where(type => type.GetInterfaces().Contains(typeof(IDocument)))
            .ToTheoryData();
    }
}
