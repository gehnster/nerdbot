using Logging;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;

namespace NLogLogger
{
    public class NLogConsoleLogger : Logging.ILogger
    {
        private Logger _log;
        public NLogConsoleLogger(string className)
        {
            SetupLogging(className);
        }

        private void SetupLogging(string className)
        {
            try
            {
                var logConfig = new LoggingConfiguration();
                var consoleTarget = new ColoredConsoleTarget() { Layout = @"${date:format=HH\:mm\:ss} ${logger} | ${message}" };

                logConfig.AddTarget("Console", consoleTarget);

                logConfig.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, consoleTarget));

                LogManager.Configuration = logConfig;
                _log = LogManager.GetLogger(className);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void DebugLogging(string d)
        {
            _log.Info(d);
        }

        public void ErrorLogging(string e)
        {
            _log.Info(e);
        }

        public void InfoLogging(string i)
        {
            _log.Info(i);
        }
    }
}
