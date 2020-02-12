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
        private readonly NLog.Logger _logger;
        private InGameLogger _inGameLogger;
        private bool _inGameMessagesEnabled = false;
        private Log()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        // Public API
        public void LogMessage(LogCategory category, string message)
        {
            LogWithCategory(category, message);
        }

        public void LogRemovePersonalInfo(LogCategory category, string message, params object[] args)
        {
            _logger.Trace(message, args);
            LogWithCategory(category, message);
        }

        public void LogException(string message, Exception ex)
        {
            _logger.Error(ex, message);
        }

        // In game messages
        public void ShowInGameMessage(string message, bool containsPersonalInfo)
        {
            if (_inGameLogger == null)
            {
                _logger.Warn("InGameLogger has not been registered");
                return;
            }

            if (!_inGameMessagesEnabled)
            {
                _logger.Warn("InGameMessages have not been enabled");
            }
            _inGameLogger.Log(message);
            
            // Only log locally if it contains no personal information
            if (!containsPersonalInfo)
            {
                _logger.Debug(message);
            }
            
        }

        public void RegisterInGameLogger(InGameLogger gameLogger)
        {
            _logger.Info("Registered InGameLogger");
            _inGameLogger = gameLogger;
        }

        public void SetInGameMessagesEnabled(bool enabled)
        {
            _inGameMessagesEnabled = enabled;
        }

        // Private Methods
        private void LogWithCategory(LogCategory category, string message)
        {
            switch (category)
            {
                case LogCategory.Trace:
                    _logger.Trace(message);
                    break;
                case LogCategory.Debug:
                    _logger.Debug(message);
                    break;
                case LogCategory.Info:
                    _logger.Info(message);
                    break;
                case LogCategory.Warn:
                    _logger.Warn(message);
                    break;
                case LogCategory.Error:
                    _logger.Error(message);
                    break;
                case LogCategory.Fatal:
                    _logger.Fatal(message);
                    break;
            }
        }
    }
}
