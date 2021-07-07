using System.IO;

namespace DotNetRu.Auditor.Storage.FileSystem.Memory
{
    internal sealed class ReusableStream : MemoryStream
    {
        public override void Close()
        {
            // Do nothing
        }

        public void RealClose()
        {
            base.Close();
        }
    }
}