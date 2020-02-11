using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.Logger
{
    public interface ILogger
    {
        void LogMessage(LogType type, string message);
        void LogMessage(LogType type, string messageFormat, params object[] args);
        void LogException(string message, Exception ex);
        void ShowInGameMessage(string message);
        void RegisterInGameLogger(InGameLogger gameLogger);
    }
}
