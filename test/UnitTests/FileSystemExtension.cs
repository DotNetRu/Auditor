using System;
using System.IO;
using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.FileSystem;

namespace DotNetRu.Auditor.UnitTests
{
    internal static class FileSystemExtension
    {
        public static async Task WriteAllTextAsync(this IFile file, string content)
        {
            var writableFile = await file.RequestWriteAccessAsync().ConfigureAwait(false);
            if (writableFile == null)
            {
                throw new InvalidOperationException($"Can't write to «{file.FullName}» file");
            }

            await writableFile.WriteAllTextAsync(content).ConfigureAwait(false);
        }

        public static async Task WriteAllTextAsync(this IWritableFile file, string content)
        {
            var fileStream = await file.OpenForWriteAsync().ConfigureAwait(false);
            await using (fileStream.ConfigureAwait(false))
            {
                var fileWriter = new StreamWriter(fileStream);
                await using (fileWriter.ConfigureAwait(false))
                {
                    await fileWriter.WriteAsync(content).ConfigureAwait(false);
                }
            }
        }
    }
}
