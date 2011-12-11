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
        /// Query to obtain an ID of the next thread in a route path.
        /// </summary>
        public const int RouteRequest = -11;

        /// <summary>
        /// Information about a route path.
        /// </summary>
        public const int RouteResponse = -12;

        /// <summary>
        /// Resolve recipients and shortcuts request.
        /// </summary>
        public const int ResolveRecipientRequest = -13;

        public const int ResolveRecipientResponse = -14;

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
