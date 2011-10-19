using System;
using System.IO;

namespace Cloudy.Protobuf.Interfaces
{
    public abstract class AbstractSerializer
    {
        public abstract void Serialize(Stream stream, object o);

        public byte[] Serialize(object o)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Serialize(stream, o);
                return stream.ToArray();
            }
        }

        public abstract object Deserialize(Stream stream);

        public object Deserialize(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                return Deserialize(stream);
            }
        }

        public virtual bool ShouldBeSkipped(object value)
        {
            return false;
        }
    }
}
