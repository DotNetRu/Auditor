using System;
using System.Collections.Generic;
using System.Linq;
using DotNetRu.Auditor.Data.Model;
using DotNetRu.Auditor.Storage;
using DotNetRu.Auditor.UnitTests.Data.Xml;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Storage.Collections
{
    public sealed class StoreOptionsTest
    {
        private readonly StoreOptions options;

        public StoreOptionsTest()
        {
            options = new StoreOptions();
        }

        [Theory]
        [MemberData(nameof(TypeWithName))]
        public void ShouldMapCollectionName(Type type, string expectedCollectionName)
        {
            // Act
            var actualCollectionName = options.MapCollectionName(type);

            // Assert
            Assert.Equal(expectedCollectionName, actualCollectionName);
        }

        [Fact]
        public void ShouldHaveAllTypes()
        {
            // Arrange
            var allModelTypes = XmlDataSerializerFactoryTest.ModelTypes;

            // Act
            var knownModelTypes = NamedTypes.Select(pair => pair.Key);

            // Assert
            Assert.Equal(allModelTypes, knownModelTypes);
        }

        public static TheoryData<Type, string> TypeWithName => NamedTypes.ToTheoryData();

        private static IReadOnlyDictionary<Type, string> NamedTypes => new Dictionary<Type, string>
        {
            { typeof(Community), "communities" },
            { typeof(Friend), "friends" },
            { typeof(Meetup), "meetups" },
            { typeof(Speaker), "speakers" },
            { typeof(Talk), "talks" },
            { typeof(Venue), "venues" }
        };
    }
}
