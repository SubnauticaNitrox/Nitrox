using System;

namespace NitroxModel.Logger
{
    public class Log
    {
        private static bool inGameMessages;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Nitrox");

        // Enable the in-game notifications
        public static void EnableInGameMessages()
        {
            inGameMessages = true;
        }

        // For in-game notifications
        public static void InGame(string msg)
        {
            if (inGameMessages)
            {
                ErrorMessage.AddMessage(msg);
                Info(msg);
            }
        }

        public static void Error(string fmt, params object[] arg)
        {
            log.Error(Format(fmt, arg));
        }

        public static void Error(string msg, Exception ex)
        {
            log.Error(msg, ex);
        }

        public static void Warn(string fmt, params object[] arg)
        {
            log.Warn(Format(fmt, arg));
        }

        public static void Info(string fmt, params object[] arg)
        {
            log.Info(Format(fmt, arg));
        }

        public static void Info(object o)
        {
            string msg = o == null ? "null" : o.ToString();
            Info(msg);
        }

        // Only for debug prints. Should not be displayed to general user.
        // Should we print the calling method for this for more debug context?
        public static void Debug(string fmt, params object[] arg)
        {
            log.Debug(Format(fmt, arg));
        }

        public static void Debug(object o)
        {
            string msg = o == null ? "null" : o.ToString();
            Debug(msg);
        }

        // Helping method for formatting string correctly with arguments
        private static string Format(string fmt, params object[] arg)
        {
            return string.Format(fmt, arg);
        }
    }
}
