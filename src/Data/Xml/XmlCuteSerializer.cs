using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace DotNetRu.Auditor.Data.Xml
{
    internal class XmlCuteSerializer
    {
        private static readonly XmlSerializerNamespaces NoNamespace = new();
        private static readonly XmlWriterSettings Settings = new();
        private readonly XmlSerializer engine;

        static XmlCuteSerializer()
        {
            NoNamespace.Add(String.Empty, String.Empty);

            Settings.OmitXmlDeclaration = true;
            Settings.Indent = true;
        }

        public XmlCuteSerializer(Type type, XmlAttributeOverrides? overrides = null)
        {
            // NOTE: When we use XmlSerializer with overrides, framework cannot reuse generated assemblies.
            // Therefore, to avoid memory leaks, we should cache all serializers with the same overrides.
            engine = new XmlSerializer(type, overrides);
        }

        public string SerializeObject(object? entity)
        {
            var document = new XmlDocument();
            var navigator = document.CreateNavigator() ?? throw new InvalidOperationException("Can't create Navigator for empty XmlDocument");

            using (var writer = navigator.AppendChild())
            {
                engine.Serialize(writer, entity, NoNamespace);
            }

            // HACK: Unfortunately, we don't have any way to skip empty
            // elements during serialization (without changing the model)
            RemoveEmptyElements(navigator);

            // HACK: navigatorWriter has ugly settings, so we have to reformat the document
            var state = PrettyFormat(document);

            return state;
        }

        public object? DeserializeObject(string state)
        {
            using var buffer = new StringReader(state);
            using var reader = XmlReader.Create(buffer);

            return engine.Deserialize(reader);
        }

        private static void RemoveEmptyElements(XPathNavigator navigator)
        {
            navigator.MoveToFollowing(XPathNodeType.Element);

            var iterator = navigator.SelectDescendants(XPathNodeType.Element, true);
            while (iterator.MoveNext())
            {
                var element = iterator.Current;
                if (element is {HasChildren: false})
                {
                    element.DeleteSelf();
                }
            }
        }

        private static string PrettyFormat(XmlDocument document)
        {
            var buffer = new StringBuilder();
            using var writer = XmlWriter.Create(buffer, Settings);
            document.Save(writer);

            return buffer.ToString();
        }
    }
}
