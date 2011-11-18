using System;

namespace Cloudy.Examples.Shared.Helpers
{
    public static class RandomExtensions
    {
        private static readonly Random Random = new Random();

        public static Random Instance
        {
            get { return Random; }
        }
    }
}
