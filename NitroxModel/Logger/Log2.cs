using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NLog.Targets.Wrappers;

namespace NitroxModel.Logger
{
    public sealed class Log2 : ILogger
    {
        // Instance
        private static ILogger instance;
        public static ILogger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Log2();
                }
                return instance;
            }
        }

        // Private  vars
        private readonly NLog.Logger logger;

        // Public API
        public Log2()
        {
            logger = LogManager.GetCurrentClassLogger();

            ConfigureLogging();
        }

        public void log(LogType type, string message)
        {
            switch (type)
            {
                case LogType.Debug:
                    logger.Debug(message);
                    break;
                case LogType.Trace:
                    logger.Trace(message);
                    break;
                case LogType.Info:
                    logger.Info(message);
                    break;
                case LogType.Warn:
                    logger.Warn(message);
                    break;
                case LogType.Error:
                    logger.Error(message);
                    break;
                case LogType.Fatal:
                    logger.Fatal(message);
                    break;
            }
        }

        /*
         Doing this config here instead of XML because it keeps the code together
         Sometimes trying to congif in XML can be a pain
        */
        private void ConfigureLogging()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "logfile.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Better to be safe by making sure we can write to the file async.
            var wrapper = new AsyncTargetWrapper(logfile, 5000, AsyncTargetWrapperOverflowAction.Discard);

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, wrapper);
            

            // Apply config           
            LogManager.Configuration = config;
        }
    }
}
