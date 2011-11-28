using System;
using System.Collections.Generic;
using System.Net;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Structures;

namespace Cloudy.Computing.Interfaces
{
    public interface INetworkRepository
    {
        void AddToTotalSlotsCount(int count);

        void RemoveFromTotalSlotsCount(int count);

        int GetTotalSlotsCount();

        void AddSlave(IPEndPoint endPoint, SlaveContext slave);

        SlaveContext GetSlave(IPEndPoint endPoint);

        bool TryGetSlave(Guid slaveId, out SlaveContext slave);

        IEnumerable<IPEndPoint> GetSlavesEndPoints();

        void RemoveSlave(IPEndPoint endPoint);

        int GetThreadsCount(Guid slaveId);

        void AddThread(Guid slaveId, ThreadContext thread);

        void RemoveThread(Guid slaveId, Guid threadId);

        IEnumerable<ThreadContext> GetThreads(Guid slaveId);

        IEnumerable<SlaveContext> GetAllSlaves();

        void SetThreadState(Guid threadId, ThreadState state);
    }
}
