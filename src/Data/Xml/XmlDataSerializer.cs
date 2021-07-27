using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DotNetRu.Auditor.Data.Xml
{
    internal sealed class XmlDataSerializer<T> : XmlCuteSerializer, IDataSerializer<T>
        where T : IRecord
    {
        public XmlDataSerializer(XmlAttributeOverrides? overrides = null)
            : base(typeof(T), overrides)
        {
        }

        public Task SerializeAsync(Stream output, T? entity)
        {
            SerializeObject(output, entity);
            return Task.CompletedTask;
        }

        public Task<T?> DeserializeAsync(Stream input)
        {
            var entity = (T?)DeserializeObject(input);
            return Task.FromResult(entity);
        }
    }
}
