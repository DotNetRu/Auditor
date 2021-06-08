namespace DotNetRu.Auditor.Data
{
    public interface IDataSerializer<T>
    {
        string Serialize(T? entity);

        T? Deserialize(string state);
    }
}
