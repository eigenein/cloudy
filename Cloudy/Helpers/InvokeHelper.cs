using System;
using System.Diagnostics;
using System.Threading;

namespace Cloudy.Helpers
{
    public static class InvokeHelper
    {
        public static bool CallWithTimeout(Action action, TimeSpan timeout)
        {
            TimeSpan timeElapsed;
            return CallWithTimeout(action, timeout, out timeElapsed);
        }

        public static bool CallWithTimeout(Action action, TimeSpan timeout,
            out TimeSpan timeElapsed)
        {
            Thread threadToKill = null;
            Stopwatch stopwatch = new Stopwatch();
            Action wrappedAction = () =>
            {
                threadToKill = Thread.CurrentThread;
                stopwatch.Start();
                action();
            };
            bool success = false;
            IAsyncResult result = wrappedAction.BeginInvoke(null, null);
            if (result.AsyncWaitHandle.WaitOne(timeout))
            {
                wrappedAction.EndInvoke(result);
                success = true;
            }
            else
            {
                threadToKill.Abort();
            }
            stopwatch.Stop();
            timeElapsed = stopwatch.Elapsed;
            return success;
        }
    }
}
