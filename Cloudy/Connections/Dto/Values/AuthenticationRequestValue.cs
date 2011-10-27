using System;

namespace Cloudy.Connections.Dto.Values
{
    public class AuthenticationRequestValue
    {
        public Guid ClientId { get; set; }

        public byte[] AuthenticationData { get; set; }
    }
}
