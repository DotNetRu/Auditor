using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

namespace DotNetRu.Auditor.Tests
{
    internal static class XmlDocumentExtension
    {
        public static IDictionary<string, string> GetXmlNamespaces(this XmlDocument document)
        {
            var navigator = AssertEx.NotNull(document.CreateNavigator());
            navigator.MoveToFollowing(XPathNodeType.Element);

            return navigator.GetNamespacesInScope(XmlNamespaceScope.All);
        }

        public static XmlDeclaration? GetXmlDeclaration(this XmlDocument document)
        {
            return document.HasChildNodes ? document.FirstChild as XmlDeclaration : null;
        }

        public static IEnumerable<XmlElement> SelectDescendantElements(this XmlElement element, bool matchSelf = false)
        {
            var navigator = AssertEx.NotNull(element.CreateNavigator());
            return navigator
                .SelectDescendants(XPathNodeType.Element, matchSelf)
                .Cast<XPathNavigator>()
                .Select(current => current.UnderlyingObject)
                .Cast<XmlElement>();
        }
    }
}
