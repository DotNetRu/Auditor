using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DotNetRu.Auditor.Data.Xml;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Data.Xml
{
    public sealed class XmlCuteSerializerFormatTest
    {
        private readonly XmlDocument document;

        public XmlCuteSerializerFormatTest()
        {
            var ticket = new Ticket
            {
                Id = "1",
                Name = "Cute",
                ShowTime = new DateTime(2015, 08, 25, 17, 30, 0, DateTimeKind.Utc)
            };

            var serializer = new XmlCuteSerializer(typeof(Ticket));
            var ticketState = serializer.SerializeObject(ticket);

            document = new XmlDocument();
            document.LoadXml(ticketState);
        }

        [Fact]
        public void ShouldWriteRoot()
        {
            var root = document.DocumentElement;

            Assert.Equal(nameof(Ticket), root?.Name);
        }

        [Theory]
        [InlineData(nameof(Ticket.Id), "1")]
        [InlineData(nameof(Ticket.Name), "Cute")]
        public void ShouldWriteProperties(string name, string value)
        {
            var id = AssertEx.NotNull(document.DocumentElement).SelectSingleNode($"{name}");

            Assert.Equal(value, id?.InnerText);
        }

        [Fact]
        public void ShouldWriteDateTimeToUtc()
        {
            var time = AssertEx.NotNull(document.DocumentElement).SelectSingleNode(nameof(Ticket.ShowTime));

            Assert.Equal("2015-08-25T17:30:00Z", time?.InnerText);
        }

        [Fact]
        public void ShouldReadDateTimeFromUtc()
        {
            // Arrange
            var expectedTime = new DateTime(2015, 08, 25, 17, 30, 0, DateTimeKind.Utc);
            const string ticketState = "<Ticket><ShowTime>2015-08-25T17:30:00Z</ShowTime></Ticket>";
            var serializer = new XmlCuteSerializer(typeof(Ticket));

            // Act
            var ticket = serializer.DeserializeObject(ticketState) as Ticket;

            // Assert
            Assert.Equal(expectedTime, ticket?.ShowTime);
            Assert.Equal(DateTimeKind.Utc, ticket?.ShowTime?.Kind);
        }

        [Fact]
        public void ShouldOmitNamespace()
        {
            var namespaces = document.GetXmlNamespaces();
            namespaces.Remove("xml");

            Assert.Empty(namespaces);
        }

        [Fact]
        public void ShouldOmitDeclaration()
        {
            var declaration = document.GetXmlDeclaration();

            Assert.Null(declaration);
        }

        [Fact]
        public void ShouldWriteNoAttributes()
        {
            var attributedNodes = SelectSelfAndDescendantElements()
                .Where(node => node.HasAttributes)
                .Select(node => node.Name);

            Assert.Empty(attributedNodes);
        }

        [Fact]
        public void ShouldWriteNoEmptyElements()
        {
            var emptyNodes = SelectSelfAndDescendantElements()
                .Where(node => !node.HasChildNodes)
                .Select(node => node.Name);

            Assert.Empty(emptyNodes);
        }

        private IEnumerable<XmlElement> SelectSelfAndDescendantElements()
        {
            var rootElement = AssertEx.NotNull(document.DocumentElement);
            return rootElement.SelectDescendantElements(true);
        }
    }
}
