using System;
using NLog;

namespace NitroxModel.Logger
{
    public sealed class Log : ILog
    {
        #region Instance
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
        #endregion

        #region Private variables
        private readonly NLog.Logger logger;
        private InGameLogger inGameLogger;
        private bool inGameMessagesEnabled = false;
        private Log()
        {
            logger = LogManager.GetCurrentClassLogger();
        }
        #endregion

        #region Public API
        public void LogMessage(LogCategory category, string message)
        {
            LogWithCategory(category, message);
        }

        public void LogSensitive(LogCategory category, string message, params object[] args)
        {
#if DEBUG
            logger.Trace(message, args);
#endif
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
        #endregion

        #region Private methods
        private void LogWithCategory(LogCategory category, string message)
        {
            switch (category)
            {
                case LogCategory.Trace:
#if DEBUG
                    logger.Trace(message);
#endif
                    break;
                case LogCategory.Debug:
#if DEBUG
                    logger.Debug(message);
#endif
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
#endregion
    }
}
