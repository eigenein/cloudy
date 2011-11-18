using System;
using System.Net;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Examples.Static.Pi.Slave
{
    internal class ExampleSlave : Computing.Slave
    {
        private readonly int slotsCount;

        public ExampleSlave(IPEndPoint localEndPoint, int slotsCount)
            : base(localEndPoint)
        {
            this.slotsCount = slotsCount;
        }

        public override int SlotsCount
        {
            get { return slotsCount; }
        }

        protected override void Run(IThreadWorld world)
        {
            throw new NotImplementedException();
        }
    }
}
