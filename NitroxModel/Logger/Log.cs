using System;
using System.IO;
using NitroxModel.Helper;

namespace NitroxModel.Logger
{
    public class Log : INitroxLogger
    {
        protected readonly TextWriter Output;

        public LogLevels AllowedLevels { get; }

        public Log(LogLevels allowedLevels, TextWriter writer)
        {
            Validate.NotNull(writer, "Attempt was made to create a logger without a TextWriter.");
            AllowedLevels = allowedLevels;
            Output = writer;
        }

        public void Info(string fmt, params object[] arg)
        {
            Write(fmt, LogLevels.Info, arg);
        }

        public void Warn(string fmt, params object[] arg)
        {
            Write(fmt, LogLevels.Warn, arg);
        }

        public void Error(string msg, Exception ex)
        {
            Write(msg + "\n{0}", LogLevels.Error, (object)ex);
        }

        public void Error(Exception ex)
        {
            Write("{0}", LogLevels.Error, (object)ex);
        }

        public void Error(string fmt, params object[] arg)
        {
            Write(fmt, LogLevels.Error, arg);
        }

        public void Debug(string format, params object[] arg)
        {
            Write(format, LogLevels.Debug, arg);
        }

        public void Trace(string fmt, params object[] arg)
        {
            Write(fmt, LogLevels.Trace, arg);
        }

        protected virtual void Write(string fmt, LogLevels levels, params object[] arg)
        {
            if ((levels & AllowedLevels) == 0)
            {
                return;
            }

            string prefix;
            switch (levels)
            {
                case LogLevels.Trace:
                    prefix = "T: ";
                    break;
                case LogLevels.Debug:
                    prefix = "D: ";
                    break;
                case LogLevels.Info:
                    prefix = "I: ";
                    break;
                case LogLevels.Warn:
                    prefix = "W: ";
                    break;
                case LogLevels.Error:
                    prefix = "E: ";
                    break;
                default:
                    prefix = "";
                    break;
            }

            Write(string.Format($"[Nitrox] {prefix}{fmt}", arg));
        }

        protected virtual void Write(string message)
        {
            Output.WriteLine(message);
        }
    }
}
