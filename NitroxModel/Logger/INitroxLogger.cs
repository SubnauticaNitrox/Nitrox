using System;

namespace NitroxModel.Logger
{
    public interface INitroxLogger
    {
        void Trace(string format, params object[] arg);
        void Debug(string format, params object[] arg);
        void Info(string format, params object[] args);
        void Warn(string format, params object[] arg);
        void Error(string format, params object[] arg);
        void Error(string message, Exception ex);
        void Error(Exception ex);
    }
}
