using System;
using System.Collections.Generic;
using System.Linq;

namespace Xunit
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

        public static void Equivalence<T>(IEnumerable<T> expected, IEnumerable<T> actual)
            where T : IComparable<T>
        {
            Assert.Equal(expected.OrderBy(i => i), actual.OrderBy(i => i));
        }
    }
}
