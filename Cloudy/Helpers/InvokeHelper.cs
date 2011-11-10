using System;
using System.Threading;

namespace Cloudy.Helpers
{
    public static class InvokeHelper
    {
        public static bool CallWithTimeout(Action action, TimeSpan timeout)
        {
            Thread threadToKill = null;
            Action wrappedAction = () =>
            {
                threadToKill = Thread.CurrentThread;
                action();
            };

            IAsyncResult result = wrappedAction.BeginInvoke(null, null);
            if (result.AsyncWaitHandle.WaitOne(timeout))
            {
                wrappedAction.EndInvoke(result);
                return true;
            }
            threadToKill.Abort();
            return false;
        }
    }
}
