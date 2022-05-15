using System.ComponentModel;

namespace NitroxLauncher.Models.Troubleshoot.Abstract
{
    public enum TroubleshootStatus
    {
        /// <summary>
        /// Troubleshoot module has not been triggered yet
        /// </summary>
        [Description("Waiting...")]
        NOT_STARTED,

        /// <summary>
        /// Troubleshoot module is currently running
        /// </summary>
        [Description("Checking...")]
        RUNNING,

        /// <summary>
        /// Troubleshoot module has ended without trouble
        /// </summary>
        [Description("OK")]
        OK,

        /// <summary>
        /// Troubleshoot module has ended with errors
        /// </summary>
        [Description("KO")]
        KO,

        /// <summary>
        /// Troubleshoot modules has encountered errors that crashed the checking process
        /// </summary>
        [Description("Crashed")]
        FATAL_ERROR
    }
}
