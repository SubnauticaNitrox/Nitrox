using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace NitroxModel.Logger
{
    public interface ILogger
    {
        void log(string message);
    }
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

        public void log(string message)
        {
            logger.Info(message);
        }

        private void ConfigureLogging()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "logfile.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Apply config           
            LogManager.Configuration = config;
        }
    }
}
