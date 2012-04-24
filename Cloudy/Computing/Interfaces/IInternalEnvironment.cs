using System;
using Cloudy.Computing.Structures.Values;

namespace Cloudy.Computing.Interfaces
{
    internal interface IInternalEnvironment : IEnvironment
    {
        /// <summary>
        /// Gets the serialized current rank.
        /// </summary>
        byte[] RawRank { get; set; }

        void NotifyValueReceived(EnvironmentOperationValue value);

        void CleanUp();

        void ResetTime();
    }
}
