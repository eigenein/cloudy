using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Networking.Values
{
    /// <summary>
    /// Holds a request for an external endpoint.
    /// </summary>
    [ProtobufSerializable]
    public class ExternalIPEndPointRequest
    {
        /// <summary>
        /// Empty constructor for deserialization.
        /// </summary>
        public ExternalIPEndPointRequest()
        {
            // Do nothing.
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id">The request ID.</param>
        public ExternalIPEndPointRequest(int id)
        {
            this.Id = id;
        }

        /// <summary>
        /// The request ID. Set by a client.
        /// </summary>
        [ProtobufField(1)]
        public int Id { get; set; }
    }
}
