using System.IO;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.UnitTests
{
    internal static class StreamExtension
    {
        public static async Task<string> ReadAllTextAsync(this Stream stream)
        {
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }
    }
}
