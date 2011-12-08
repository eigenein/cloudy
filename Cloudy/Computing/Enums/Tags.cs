﻿using System;

namespace Cloudy.Computing.Enums
{
    public static class Tags
    {
        public const int JoinRequest = -3;

        public const int JoinResponse = -4;

        public const int Bye = -5;

        public const int StartThread = -6;

        public const int StopThread = -7;

        public const int ThreadCompleted = -8;

        public const int ThreadFailed = -9;

        /// <summary>
        /// Computing environment operation data.
        /// </summary>
        public const int EnvironmentOperation = -10;
    }
}
