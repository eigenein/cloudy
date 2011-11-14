using System;
using System.Configuration;
using NLog;

namespace Cloudy.Examples.Chat.Shared
{
    public static class Configuration
    {
        private static readonly Logger Logger =
            LogManager.GetCurrentClassLogger();

        private static T Get<T>(string key, Func<string, T> parseFunc)
        {
            try
            {
                return parseFunc(ConfigurationManager.AppSettings[key]);
            }
            catch (Exception ex)
            {
                Logger.Error("{2} {0}: {1}", key, ex.Message, typeof(T).Name);
                throw;
            }
        }

        public static int GetInt32(string key)
        {
            return Get(key, Int32.Parse);
        }
    }
}
