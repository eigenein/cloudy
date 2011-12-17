using System;

namespace Cloudy.Computing.Enums
{
    public static class Tags
    {
        public const int JoinRequest = -3;

        public const int JoinResponse = -4;

        /// <summary>
        /// Node leaving notification.
        /// </summary>
        public const int Bye = -5;

        /// <summary>
        /// Thread Start command.
        /// </summary>
        public const int StartThread = -6;

        /// <summary>
        /// Thread Stop command.
        /// </summary>
        public const int StopThread = -7;

        /// <summary>
        /// Thread Completed notification.
        /// </summary>
        public const int ThreadCompleted = -8;

        /// <summary>
        /// Thread Completed notification.
        /// </summary>
        public const int ThreadFailed = -9;

        /// <summary>
        /// Computing environment operation data.
        /// </summary>
        public const int EnvironmentOperation = -10;

        /// <summary>
        /// Reassigns an existing thread to other rank.
        /// </summary>
        public const int ReassignRank = -11;

        /// <summary>
        /// Request IP endpoint by a thread ID.
        /// </summary>
        public const int EndPointRequest = -15;

        public const int EndPointResponse = -16;

        /// <summary>
        /// Same as <c>Ping</c> but with a sender ID.
        /// </summary>
        public const int SignedPing = -17;

        /// <summary>
        /// Request for a ping by another thread.
        /// </summary>
        public const int SignedPingRequest = -18;

        public const int SignedPingResponse = -19;
    }
}
