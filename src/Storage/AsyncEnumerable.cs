using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.Storage
{
    internal static class AsyncEnumerable
    {
        public static IAsyncEnumerable<T> Empty<T>()
        {
            return EmptyAsyncEnumerable<T>.Instance;
        }

        private class EmptyAsyncEnumerable<T> : IAsyncEnumerable<T>
        {
            public static readonly IAsyncEnumerable<T> Instance = new EmptyAsyncEnumerable<T>();

            public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new())
            {
                await Task.Yield();
                yield break;
            }
        }
    }
}
