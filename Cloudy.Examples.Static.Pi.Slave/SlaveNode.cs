﻿using System;
using System.Net;
using Cloudy.Computing;
using Cloudy.Computing.Interfaces;
using NLog;

namespace Cloudy.Examples.Static.Pi.Slave
{
    public class SlaveNode : AbstractSlaveNode
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly int slotsCount;

        public SlaveNode(int port, IPAddress localAddress, int slotsCount) 
            : base(port, localAddress, () => new ComputingThread(Run))
        {
            this.slotsCount = slotsCount;

            StateChanged += (sender, e) =>
                Logger.Info("State: {0}", e.Value);
            Joined += (sender, e) =>
                Logger.Info("Joined: {0} -> {1}", e.Value1, e.Value2);
            ThreadStarted += (sender, e) =>
                Logger.Info("Thread Started: {0}", e.Value);
            ThreadStopped += (sender, e) =>
                Logger.Info("Thread Stopped: {0}", e.Value);
            ExceptionUnhandled += (sender, e) =>
                Logger.Error("Unhandled Exception: {0}", e.Value.ToString());
        }

        private static void Run(IEnvironment environment)
        {
            // TODO: Implement.
            Logger.Info("RUNNING");
        }

        #region Overrides of AbstractSlaveNode

        public override int SlotsCount
        {
            get { return slotsCount; }
        }

        #endregion
    }
}
