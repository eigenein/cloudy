using System;
using System.IO;

namespace Cloudy.Nodes
{
    /// <summary>
    /// Represents an abstract node.
    /// </summary>
    public abstract class Slave : Client
    {
        protected Slave(Stream controlStream, Guid id) 
            : base(controlStream, id)
        {
        }
    }
}
