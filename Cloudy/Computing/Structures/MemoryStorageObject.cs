using System;
using Cloudy.Computing.Enums;

namespace Cloudy.Computing.Structures
{
    internal class MemoryStorageObject
    {
        public object Value { get; set; }

        public TimeToLive TimeToLive { get; set; }
    }
}
