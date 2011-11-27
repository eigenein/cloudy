using System;
using System.Collections.Generic;
using System.Net;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Structures;

namespace Cloudy.Computing.Interfaces
{
    public interface IMasterRepository
    {
        void AddToTotalSlotsCount(int count);

        void RemoveFromTotalSlotsCount(int count);

        int GetTotalSlotsCount();

        void AddSlave(IPEndPoint endPoint, SlaveContext slave);

        SlaveContext GetSlave(IPEndPoint endPoint);

        bool TryGetSlave(Guid slaveId, out SlaveContext slave);

        IEnumerable<EndPoint> GetSlavesEndPoints();

        void RemoveSlave(IPEndPoint endPoint);

        int GetThreadsCount(Guid slaveId);

        void AddThread(Guid slaveId, ThreadContext thread);

        IEnumerable<ThreadContext> GetThreads(Guid slaveId);

        IEnumerable<SlaveContext> GetAllSlaves();

        void SetThreadState(Guid threadId, ThreadState state);
    }
}
