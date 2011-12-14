using System;

namespace Cloudy.Helpers
{
    public static class ByteArrayExtensions
    {
        public static bool SameAs(this byte[] array, byte[] anotherArray)
        {
            if (array.Length != anotherArray.Length)
            {
                return false;
            }
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != anotherArray[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
