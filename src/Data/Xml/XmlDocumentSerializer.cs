using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DotNetRu.Auditor.Data.Xml
{
    internal sealed class XmlDocumentSerializer<T> : XmlCuteSerializer, IDocumentSerializer<T>
        where T : IDocument
    {
        public XmlDocumentSerializer(XmlAttributeOverrides? overrides = null)
            : base(typeof(T), overrides)
        {
        }

        public Task SerializeAsync(Stream output, T? document)
        {
            SerializeObject(output, document);
            return Task.CompletedTask;
        }

        public Task<T?> DeserializeAsync(Stream input)
        {
            var entity = (T?)DeserializeObject(input);
            return Task.FromResult(entity);
        }
    }
}
