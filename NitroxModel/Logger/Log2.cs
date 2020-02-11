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
        private readonly NLog.Logger _logger;
        private InGameLogger _inGameLogger;
        private Log2()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        // Public API
        public void LogMessage(LogType type, string message)
        {
            switch (type)
            {
                case LogType.Debug:
                    _logger.Debug(message);
                    break;
                case LogType.Trace:
                    _logger.Trace(message);
                    break;
                case LogType.Info:
                    _logger.Info(message);
                    break;
                case LogType.Warn:
                    _logger.Warn(message);
                    break;
                case LogType.Error:
                    _logger.Error(message);
                    break;
                case LogType.Fatal:
                    _logger.Fatal(message);
                    break;
            }
        }

        public void LogMessage(LogType type, string messageFormat, params object[] args)
        {
            switch (type)
            {
                case LogType.Debug:
                    _logger.Debug(Format(messageFormat, args));
                    break;
                case LogType.Trace:
                    _logger.Trace(Format(messageFormat, args));
                    break;
                case LogType.Info:
                    _logger.Info(Format(messageFormat, args));
                    break;
                case LogType.Warn:
                    _logger.Warn(Format(messageFormat, args));
                    break;
                case LogType.Error:
                    _logger.Error(Format(messageFormat, args));
                    break;
                case LogType.Fatal:
                    _logger.Fatal(Format(messageFormat, args));
                    break;
            }
        }

        public void LogException(string message, Exception ex)
        {
            _logger.Error(ex, message);
        }

        public void ShowInGameMessage(string message)
        {
            if (_inGameLogger == null)
            {
                _logger.Warn("InGameLogger has not been registered");
                return;
            }
            _inGameLogger.Log(message);
            _logger.Debug(message);
        }

        public void RegisterInGameLogger(InGameLogger gameLogger)
        {
            _logger.Info("Registered InGameLogger");
            _inGameLogger = gameLogger;
        }

        // Private methods

        private string Format(string format, params object[] args)
        {
            return string.Format(format, args);
        }
    }
}
