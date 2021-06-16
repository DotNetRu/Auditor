using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DotNetRu.Auditor.Data.Model;
using DotNetRu.Auditor.Data.Xml;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Data.Xml
{
    public sealed class XmlDataSerializerFactoryTest
    {
        [Theory]
        [MemberData(nameof(ModelTypesAsSingleArgument))]
        [SuppressMessage("ReSharper", "xUnit1026")]
        public void ShouldKnowAboutTheWholeModel2<T>(T _)
        {
            // Act
            var builder = XmlDataSerializerFactory.CreateModelBuilder<T>();

            // Assert
            Assert.NotNull(builder);
            Assert.NotNull(builder.Build());
        }

        public static IEnumerable<object[]> ModelTypesAsSingleArgument =>
            ModelTypes.Select(type => new[] {Activator.CreateInstance(type)!});

        private static IReadOnlyList<Type> ModelTypes => new List<Type>
        {
            typeof(CommunityRecord),
            typeof(MeetupRecord),
            typeof(SpeakerRecord),
            typeof(TalkRecord),
            typeof(VenueRecord),
            typeof(FriendRecord)
        };
    }
}
