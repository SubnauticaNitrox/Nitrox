using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.Logger
{
    public interface ILogger
    {
        void LogMessage(NLogType type, string message);
        void LogMessageWithPersonal(NLogType type, string message, params object[] args);
        void LogException(string message, Exception ex);
        
        // In game messaging
        void ShowInGameMessage(string message, bool containsPersonalInfo = false);
        void RegisterInGameLogger(InGameLogger gameLogger);
        void SetInGameMessagesEnabled(bool enabled); 
    }
}
