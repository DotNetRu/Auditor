using System.IO;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.Data
{
    public interface IDataSerializer<T>
        where T : IRecord
    {
        Task SerializeAsync(Stream output, T? entity);

        Task<T?> DeserializeAsync(Stream input);
    }
}
