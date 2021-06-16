using System;
using Xunit;

namespace DotNetRu.Auditor.UnitTests
{
    internal static class AssertEx
    {
        // TODO: Remove after migrating to xUnit v3
        public static T NotNull<T>(T? value)
        {
            if (value == null)
            {
                Assert.NotNull(value);
                throw new InvalidProgramException("Impossible");
            }

            return value;
        }
    }
}
