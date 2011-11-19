using System;
using Cloudy.Computing.Topologies.Structures;

namespace Cloudy.Computing.Topologies.Interfaces
{
    public interface IRouter
    {
        /// <summary>
        /// Gets the next address for routing of a message from the current
        /// thread to the destination thread.
        /// </summary>
        ThreadAddress GetRoute(ThreadAddress current, ThreadAddress destination);
    }
}
