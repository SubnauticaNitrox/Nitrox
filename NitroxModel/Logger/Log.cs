using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NLog.Targets.Wrappers;

namespace NitroxModel.Logger
{
    public sealed class Log : ILog
    {
        // Instance
        private static ILog instance;
        public static ILog Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Log();
                }
                return instance;
            }
        }

        // Private  variables
        private readonly NLog.Logger logger;
        private InGameLogger inGameLogger;
        private bool inGameMessagesEnabled = false;
        private Log()
        {
            logger = LogManager.GetCurrentClassLogger();
        }

        // Public API
        public void LogMessage(LogCategory category, string message)
        {
            LogWithCategory(category, message);
        }

        public void LogRemovePersonalInfo(LogCategory category, string message, params object[] args)
        {
            logger.Trace(message, args);
            LogWithCategory(category, message);
        }

        public void LogException(string message, Exception ex)
        {
            logger.Error(ex, message);
        }

        // In game messages
        public void ShowInGameMessage(string message, bool containsPersonalInfo)
        {
            if (inGameLogger == null)
            {
                logger.Warn("InGameLogger has not been registered");
                return;
            }

            if (!inGameMessagesEnabled)
            {
                logger.Warn("InGameMessages have not been enabled");
            }
            inGameLogger.Log(message);
            
            // Only log locally if it contains no personal information
            if (!containsPersonalInfo)
            {
                logger.Debug(message);
            }
            
        }

        public void RegisterInGameLogger(InGameLogger gameLogger)
        {
            logger.Info("Registered InGameLogger");
            inGameLogger = gameLogger;
        }

        public void SetInGameMessagesEnabled(bool enabled)
        {
            inGameMessagesEnabled = enabled;
        }

        // Private Methods
        private void LogWithCategory(LogCategory category, string message)
        {
            switch (category)
            {
                case LogCategory.Trace:
                    logger.Trace(message);
                    break;
                case LogCategory.Debug:
                    logger.Debug(message);
                    break;
                case LogCategory.Info:
                    logger.Info(message);
                    break;
                case LogCategory.Warn:
                    logger.Warn(message);
                    break;
                case LogCategory.Error:
                    logger.Error(message);
                    break;
                case LogCategory.Fatal:
                    logger.Fatal(message);
                    break;
            }
        }
    }
}
