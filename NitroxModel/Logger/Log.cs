using System;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using log4net.Util;

namespace NitroxModel.Logger
{
    public class Log
    {
        private static bool inGameMessages;

        private static readonly ILog log = LogManager.GetLogger("Nitrox");

        static Log()
        {
            Setup();
        }

        // Enable the in-game notifications
        public static void EnableInGameMessages()
        {
            inGameMessages = true;
        }

        // For in-game notifications
        public static void InGame(string msg)
        {
            if (inGameMessages)
            {
                ErrorMessage.AddMessage(msg);
                Info(msg);
            }
        }

        public static void Error(string fmt, params object[] arg)
        {
            log.Error(Format(fmt, arg));
        }

        public static void Error(string msg, Exception ex)
        {
            log.Error(msg, ex);
        }

        public static void Warn(string fmt, params object[] arg)
        {
            log.Warn(Format(fmt, arg));
        }

        public static void Info(string fmt, params object[] arg)
        {
            log.Info(Format(fmt, arg));
        }

        public static void Info(object o)
        {
            string msg = o == null ? "null" : o.ToString();
            Info(msg);
        }

        // Only for debug prints. Should not be displayed to general user.
        // Should we print the calling method for this for more debug context?
        public static void Debug(string fmt, params object[] arg)
        {
            log.Debug(Format(fmt, arg));
        }

        public static void Debug(object o)
        {
            string msg = o == null ? "null" : o.ToString();
            Debug(msg);
        }

        // Helping method for formatting string correctly with arguments
        private static string Format(string fmt, params object[] arg)
        {
            return string.Format(fmt, arg);
        }

        private static void Setup()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "[%d{HH:mm:ss} %level]: %m%n";
            patternLayout.ActivateOptions();

            LevelRangeFilter filter = new LevelRangeFilter();
            filter.LevelMin = Level.Debug;
            filter.LevelMax = Level.Fatal;

            RollingFileAppender fileAppender = new RollingFileAppender();
            fileAppender.File = "Nitrox Logs/nitrox-.log";
            fileAppender.AppendToFile = false;
            fileAppender.RollingStyle = RollingFileAppender.RollingMode.Date;
            fileAppender.MaxSizeRollBackups = 10;
            fileAppender.DatePattern = "yyyy-MM-dd";
            fileAppender.StaticLogFileName = false;
            fileAppender.PreserveLogFileNameExtension = true;
            fileAppender.LockingModel = new FileAppender.MinimalLock();
            fileAppender.Layout = patternLayout;
            fileAppender.ActivateOptions();
            fileAppender.AddFilter(filter);

            ConsoleAppender consoleAppender = new ConsoleAppender();
            consoleAppender.Layout = patternLayout;
            consoleAppender.AddFilter(filter);

            hierarchy.Root.AddAppender(consoleAppender);
            hierarchy.Root.AddAppender(fileAppender);

            hierarchy.Configured = true;
        }
    }
}
