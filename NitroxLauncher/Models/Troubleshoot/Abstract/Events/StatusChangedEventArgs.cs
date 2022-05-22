using System;

namespace NitroxLauncher.Models.Troubleshoot.Abstract.Events
{
    [Serializable]
    public class StatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Name of the troubleshoot module that raised the event
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// Status of the troubleshoot module before the event
        /// </summary>
        public TroubleshootStatus OldStatus { get; set; }

        /// <summary>
        /// Status of the troubleshoot module after the event
        /// </summary>
        public TroubleshootStatus NewStatus { get; set; }

        public StatusChangedEventArgs(string moduleName, TroubleshootStatus oldStatus, TroubleshootStatus newStatus)
        {
            ModuleName = moduleName;
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }
    }

    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);
}
