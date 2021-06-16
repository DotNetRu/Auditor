using System;
using DotNetRu.Auditor.Data.Xml;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Data.Xml
{
    public sealed class XmlCuteSerializerMatureTest
    {
        private readonly XmlCuteSerializer serializer;

        public XmlCuteSerializerMatureTest()
        {
            serializer = new XmlCuteSerializer(typeof(Ticket));
        }

        [Fact]
        public void ShouldRebirthModel()
        {
            // Arrange
            var ticket = CreateFullTicket();

            // Act
            var ticketState = serializer.SerializeObject(ticket);
            var newTicket = serializer.DeserializeObject(ticketState);

            // Assert
            Assert.Equal(ticket, newTicket);
        }

        private static Ticket CreateFullTicket()
        {
            return new()
            {
                Id = "Mature-42",
                Name = "Mature",
                SpeakerIds = { "A", "B", "C" },
                SeatNumber = 42,
                ShowTime = DateTime.UtcNow,
                Perks =
                {
                    new TicketPerk { Name = "One", Volume = 0 },
                    new TicketPerk { Name = "Two", Volume = null },
                    new TicketPerk { Name = "Three", Volume = 1 }
                }
            };
        }
    }
}
