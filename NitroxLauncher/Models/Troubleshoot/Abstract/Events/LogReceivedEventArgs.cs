using System;

namespace NitroxLauncher.Models.Troubleshoot.Abstract.Events
{
    [Serializable]
    public class LogSentEventArgs : EventArgs
    {
        /// <summary>
        /// Name of the troubleshoot module that raised the event
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// Informations sent by a troubleshoot module
        /// </summary>
        public string Message { get; set; }

        public LogSentEventArgs(string moduleName, string message)
        {
            ModuleName = moduleName;
            Message = message;
        }
    }

    public delegate void LogSentEventHandler(object sender, LogSentEventArgs e);
}
