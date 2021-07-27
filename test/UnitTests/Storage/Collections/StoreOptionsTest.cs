using System;
using System.Collections.Generic;
using System.Linq;
using DotNetRu.Auditor.Data.Model;
using DotNetRu.Auditor.Storage.Collections;
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
        [MemberData(nameof(RecordsWithName))]
        public void ShouldMapCollectionName(Type recordType, string expectedCollectionName)
        {
            // Act
            var actualCollectionName = options.MapCollectionName(recordType);

            // Assert
            Assert.Equal(expectedCollectionName, actualCollectionName);
        }

        [Fact]
        public void ShouldCheckAllRecords()
        {
            // Arrange
            var allRecordTypes = XmlDataSerializerFactoryTest.ModelTypes;

            // Act
            var namedRecordType = NamedRecords.Select(pair => pair.Key);

            // Assert
            Assert.Equal(allRecordTypes, namedRecordType);
        }

        public static TheoryData<Type, string> RecordsWithName => NamedRecords.ToTheoryData();

        private static IReadOnlyDictionary<Type, string> NamedRecords => new Dictionary<Type, string>
        {
            { typeof(CommunityRecord), "communities" },
            { typeof(FriendRecord), "friends" },
            { typeof(MeetupRecord), "meetups" },
            { typeof(SpeakerRecord), "speakers" },
            { typeof(TalkRecord), "talks" },
            { typeof(VenueRecord), "venues" }
        };
    }
}
