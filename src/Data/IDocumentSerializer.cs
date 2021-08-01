using System.IO;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.Data
{
    public interface IDocumentSerializer<T>
        where T : IDocument
    {
        Task SerializeAsync(Stream output, T? document);

        Task<T?> DeserializeAsync(Stream input);
    }
}
