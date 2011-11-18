using System;
using System.Configuration;
using System.Net;
using NLog;

namespace Cloudy.Examples.Shared.Configuration
{
    public static class ApplicationSettings
    {
        private static readonly Logger Logger =
            LogManager.GetCurrentClassLogger();

        private static T GetValue<T>(string key, Func<string, T> convert)
        {
            string value = ConfigurationManager.AppSettings[key];
            try
            {
                return convert(value);
            }
            catch (Exception)
            {
                Logger.Error("Convert error: key = {0}, value = {1}",
                    key, value);
                throw;
            }
        }

        public static int GetInteger(string key)
        {
            return GetValue(key, Convert.ToInt32);
        }

        public static IPAddress GetIPAddress(string key)
        {
            return GetValue(key, IPAddress.Parse);
        }
    }
}
