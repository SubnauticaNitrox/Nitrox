using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.Logger
{
    public interface ILog
    {
        void LogMessage(LogCategory type, string message);
        void LogSensitive(LogCategory category, string message, params object[] args);
        void LogException(string message, Exception ex);
        
        // In game messaging
        void ShowInGameMessage(string message, bool containsPersonalInfo = false);
        void RegisterInGameLogger(InGameLogger gameLogger);
        void SetInGameMessagesEnabled(bool enabled); 
    }
}
