using System.Xml.Serialization;

namespace DotNetRu.Auditor.Data.Xml
{
    internal sealed class XmlDataSerializer<T> : XmlCuteSerializer, IDataSerializer<T>
        where T : class
    {
        public XmlDataSerializer(XmlAttributeOverrides? overrides = null)
            : base(typeof(T), overrides)
        {
        }

        public string Serialize(T? entity) => SerializeObject(entity);

        public T? Deserialize(string state) => (T?)DeserializeObject(state);
    }
}
