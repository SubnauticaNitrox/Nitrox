using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.Logger
{
    public interface ILogger
    {
        void Log(NLogType type, string message);
        void LogRemovePersonalInfo(NLogType type, string message, params object[] args);
        void LogException(string message, Exception ex);
        
        // In game messaging
        void ShowInGameMessage(string message, bool containsPersonalInfo = false);
        void RegisterInGameLogger(InGameLogger gameLogger);
        void SetInGameMessagesEnabled(bool enabled); 
    }
}
