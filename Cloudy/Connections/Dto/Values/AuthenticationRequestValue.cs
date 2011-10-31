using System;

namespace Cloudy.Connections.Dto.Values
{
    /// <summary>
    /// Carries authentication request information.
    /// </summary>
    public class AuthenticationRequestValue
    {
        public Guid ClientId { get; set; }

        public byte[] AuthenticationData { get; set; }
    }
}
