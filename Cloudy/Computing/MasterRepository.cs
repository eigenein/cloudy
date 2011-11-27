﻿using System;
using System.Collections.Generic;
using System.Net;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Structures;

namespace Cloudy.Computing
{
    public class MasterRepository : IMasterRepository
    {
        private readonly object locker = new object();

        private readonly Dictionary<IPEndPoint, SlaveContext> endPointToSlaveContext =
            new Dictionary<IPEndPoint, SlaveContext>();

        private readonly Dictionary<Guid, SlaveContext> slaveIdToSlaveContext =
            new Dictionary<Guid, SlaveContext>();

        private readonly Dictionary<Guid, List<ThreadContext>> slaveIdToThreads =
            new Dictionary<Guid, List<ThreadContext>>();

        private readonly Dictionary<Guid, ThreadContext> threadIdToThread =
            new Dictionary<Guid, ThreadContext>();

        private int totalSlotsCount;

        #region Implementation of IMasterRepository

        public void AddToTotalSlotsCount(int count)
        {
            lock (locker)
            {
                totalSlotsCount += count;
            }
        }

        public void RemoveFromTotalSlotsCount(int count)
        {
            lock (locker)
            {
                totalSlotsCount -= count;
            }
        }

        public int GetTotalSlotsCount()
        {
            lock (locker)
            {
                return totalSlotsCount;
            }
        }

        public void AddSlave(IPEndPoint endPoint, SlaveContext slave)
        {
            lock (locker)
            {
                endPointToSlaveContext.Add(endPoint, slave);
                slaveIdToSlaveContext.Add(slave.SlaveId, slave);
            }
        }

        public SlaveContext GetSlave(IPEndPoint endPoint)
        {
            return endPointToSlaveContext[endPoint];
        }

        public bool TryGetSlave(Guid slaveId, out SlaveContext slave)
        {
            return slaveIdToSlaveContext.TryGetValue(slaveId, out slave);
        }

        public IEnumerable<EndPoint> GetSlavesEndPoints()
        {
            lock (locker)
            {
                foreach (IPEndPoint endPoint in endPointToSlaveContext.Keys)
                {
                    yield return endPoint;
                }
            }
        }

        public void RemoveSlave(IPEndPoint endPoint)
        {
            lock (locker)
            {
                SlaveContext slave = endPointToSlaveContext[endPoint];
                endPointToSlaveContext.Remove(endPoint);
                slaveIdToSlaveContext.Remove(slave.SlaveId);
            }
        }

        public int GetThreadsCount(Guid slaveId)
        {
            lock (locker)
            {
                List<ThreadContext> list;
                if (!slaveIdToThreads.TryGetValue(slaveId, out list))
                {
                    return 0;
                }
                return list.Count;
            }
        }

        public void AddThread(Guid slaveId, ThreadContext thread)
        {
            lock (locker)
            {
                List<ThreadContext> list;
                if (!slaveIdToThreads.TryGetValue(slaveId, out list))
                {
                    list = slaveIdToThreads[slaveId] = new List<ThreadContext>();
                }
                list.Add(thread);
                threadIdToThread[thread.ThreadId] = thread;
            }
        }

        public IEnumerable<ThreadContext> GetThreads(Guid slaveId)
        {
            lock (locker)
            {
                List<ThreadContext> list;
                if (slaveIdToThreads.TryGetValue(slaveId, out list))
                {
                    return list;
                }
                return new List<ThreadContext>();
            }
        }

        public IEnumerable<SlaveContext> GetAllSlaves()
        {
            lock (locker)
            {
                foreach (SlaveContext slave in slaveIdToSlaveContext.Values)
                {
                    yield return slave;
                }
            }
        }

        public void SetThreadState(Guid threadId, ThreadState state)
        {
            lock (locker)
            {
                threadIdToThread[threadId].State = state;
            }
        }

        #endregion
    }
}
