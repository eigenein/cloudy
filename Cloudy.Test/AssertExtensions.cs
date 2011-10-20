using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Cloudy.Test
{
    public static class AssertExtensions
    {
        public static void AreEqual<T>(ICollection<T> expected,
            ICollection<T> actual)
        {
            if (expected.Count != actual.Count)
            {
                Assert.Fail("Length mismatch: expected: {0}, but was: {1}",
                    expected.Count, actual.Count);
            }
            IEnumerator<T> expectedEnumerator = expected.GetEnumerator();
            IEnumerator<T> actualEnumerator = actual.GetEnumerator();
            for (int i = 0; i < expected.Count; i++)
            {
                expectedEnumerator.MoveNext();
                actualEnumerator.MoveNext();
                if (!expectedEnumerator.Current.Equals(actualEnumerator.Current))
                {
                    Assert.Fail("Collections differ at position {0}. Expected: {1}, but was: {2}",
                        i, expectedEnumerator.Current, actualEnumerator.Current);
                }
            }
        }
    }
}
