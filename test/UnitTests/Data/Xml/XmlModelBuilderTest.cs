using System;
using System.Linq;
using System.Xml;
using DotNetRu.Auditor.Data.Xml;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Data.Xml
{
    public sealed class XmlModelBuilderTest
    {
        private readonly XmlElement root;
        private readonly string? groupName;

        public XmlModelBuilderTest()
        {
            var ticket = new Ticket
            {
                Id = "A1",
                Name = "The One",
                SpeakerIds = { "A", "B", "C" },
                Perks =
                {
                    new TicketPerk { Name = "One", Volume = 0 },
                    new TicketPerk { Name = "Two", Volume = null },
                    new TicketPerk { Name = "Three", Volume = 1 }
                }
            };

            var model = XmlModelBuilder<Ticket>
                .Map("OneWayTicket", "OneWayTickets")
                .Property(map => map.Id, "Number")
                .Collection(map => map.SpeakerIds, "Speakers", "Speaker")
                .Collection(map => map.Perks, "SubPerks", "SubPerk", perkModel =>
                {
                    perkModel
                        .Property(perkMap => perkMap.Name, "Title")
                        .Property(perkMap => perkMap.Volume, "Duration");
                });

            groupName = model.GroupName;
            var serializer = new XmlCuteSerializer(typeof(Ticket), model.Overrides);
            var ticketState = serializer.SerializeObject(ticket);

            var document = new XmlDocument();
            document.LoadXml(ticketState);

            root = AssertEx.NotNull(document.DocumentElement);
        }

        [Fact]
        public void ShouldRenameRoot()
        {
            Assert.Equal("OneWayTicket", root.Name);
        }

        [Fact]
        public void ShouldRenameGroup()
        {
            Assert.Equal("OneWayTickets", groupName);
        }

        [Fact]
        public void ShouldRenameProperty()
        {
            var number = root.SelectSingleNode("Number");

            Assert.Equal("A1", number?.InnerText);
        }

        [Fact]
        public void ShouldSkipNotMappedProperties()
        {
            var name = root.SelectSingleNode(nameof(Ticket.Name));

            Assert.Null(name);
        }

        [Fact]
        public void ShouldRenamePrimitiveCollectionAndItems()
        {
            var speakersElement = root.SelectSingleNode("Speakers") as XmlElement;
            var speakers = AssertEx.NotNull(speakersElement).SelectDescendantElements().ToList();

            Assert.Equal(new[] { "Speaker", "Speaker", "Speaker" } , speakers.Select(speaker => speaker.Name));
            Assert.Equal(new[] { "A", "B", "C" } , speakers.Select(speaker => speaker.InnerText));
        }

        [Fact]
        public void ShouldRenameObjectCollectionAndItems()
        {
            var speakersElement = root.SelectSingleNode("SubPerks") as XmlElement;
            var perkNames = AssertEx.NotNull(speakersElement).SelectNodes("SubPerk/Title")?.Cast<XmlNode>() ?? Array.Empty<XmlNode>();
            var perkVolumes = AssertEx.NotNull(speakersElement).SelectNodes("SubPerk/Duration")?.Cast<XmlNode>() ?? Array.Empty<XmlNode>();

            Assert.Equal(new[] { "One", "Two", "Three" } , perkNames.Select(speaker => speaker.InnerText));
            Assert.Equal(new[] { "0", "1" } , perkVolumes.Select(speaker => speaker.InnerText));
        }
    }
}
