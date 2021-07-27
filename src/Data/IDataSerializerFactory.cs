namespace DotNetRu.Auditor.Data
{
    public interface IDataSerializerFactory
    {
        IDataSerializer<T> Create<T>()
            where T : IRecord;
    }
}
