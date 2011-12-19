using System;
using Cloudy.Computing.Enums;
using Cloudy.Messaging.Structures;

namespace Cloudy.Computing.Structures
{
    public class RemoteMemoryAccessValue : ByteArrayValue
    {
        public TimeToLive TimeToLive { get; set; }
    }
}
