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

        // Private  variables
        private readonly NLog.Logger _logger;
        private InGameLogger _inGameLogger;
        private bool _inGameMessagesEnabled = false;
        private Log2()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        // Public API
        public void LogMessage(NLogType type, string message)
        {
            switch (type)
            {
                case NLogType.Debug:
                    _logger.Debug(message);
                    break;
                case NLogType.Trace:
                    _logger.Trace(message);
                    break;
                case NLogType.Info:
                    _logger.Info(message);
                    break;
                case NLogType.Warn:
                    _logger.Warn(message);
                    break;
                case NLogType.Error:
                    _logger.Error(message);
                    break;
                case NLogType.Fatal:
                    _logger.Fatal(message);
                    break;
            }
        }

        public void LogMessageWithPersonal(NLogType type, string message, params object[] args)
        {
            switch (type)
            {
                case NLogType.Debug:
                    _logger.Debug(Format(message, args));
                    break;
                case NLogType.Trace:
                    _logger.Trace(Format(message, args));
                    break;
                case NLogType.Info:
                    _logger.Info(Format(message, args));
                    break;
                case NLogType.Warn:
                    _logger.Warn(Format(message, args));
                    break;
                case NLogType.Error:
                    _logger.Error(Format(message, args));
                    break;
                case NLogType.Fatal:
                    _logger.Fatal(Format(message, args));
                    break;
            }
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
                _logger.Warn("InGameMessages has not been enabled");
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

        // Private methods

        private string Format(string format, params object[] args)
        {
            return string.Format(format, args);
        }
    }
}
