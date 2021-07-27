using System.Collections;
using System.Collections.Generic;

namespace DotNetRu.Auditor.UnitTests
{
    public abstract class TheoryData : IEnumerable<object?[]>
    {
        private readonly List<object?[]> data = new();

        protected void AddData(params object?[] values)
        {
            data.Add(values);
        }

        public IEnumerator<object?[]> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class TheoryData<T> : TheoryData
    {
        public void Add(T value) => AddData(value);

        public void AddRange(IEnumerable<T> values)
        {
            foreach (var value in values)
            {
                Add(value);
            }
        }
    }

    public class TheoryData<T1, T2> : TheoryData
    {
        public void Add(T1 value1, T2 value2) => AddData(value1, value2);

        public void AddRange(IEnumerable<KeyValuePair<T1, T2>> values)
        {
            foreach (var (key, value) in values)
            {
                Add(key, value);
            }
        }
    }

    public class TheoryData<T1, T2, T3> : TheoryData
    {
        public void Add(T1 value1, T2 value2, T3 value3) => AddData(value1, value2, value3);
    }

    public static class TheoryDataExtensions
    {
        public static TheoryData<T> ToTheoryData<T>(this IEnumerable<T> values)
        {
            var theoryData = new TheoryData<T>();
            theoryData.AddRange(values);
            return theoryData;
        }

        public static TheoryData<TKey, TValue> ToTheoryData<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> values)
        {
            var theoryData = new TheoryData<TKey, TValue>();
            theoryData.AddRange(values);
            return theoryData;
        }
    }
}
