using System;
using System.Net;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Examples.Static.Pi.Slave
{
    internal class ExampleSlave : Computing.Slave
    {
        public ExampleSlave(IPEndPoint localEndPoint)
            : base(localEndPoint)
        {
            // Do nothing.
        }

        protected override void Run(IThreadWorld world)
        {
            throw new NotImplementedException();
        }
    }
}
