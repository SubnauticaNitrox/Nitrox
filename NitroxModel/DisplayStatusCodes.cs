using System.Windows.Forms;
using static NitroxModel.DisplayStatusCodes;
namespace NitroxModel
{
    public class DisplayStatusCodes
    {
        // List all possible status codes, might be a better way to do something repetive like this(could add more descriptive names too)
        public enum StatusCode
        {
            success,
            cancelled,
            portNotListening,
            miscUnhandledException,
            saveReadErrFatal,
            privilegesErr,
            saveReadErrNonFatal,
            fileSystemErr,
            missingFeature,
            deadPiratesTellNoTales,
            invalidIP,
            processAlreadyRunning,
            invalidVariableVal,
            internetConnectFailLauncher,
            injectionFail,
            firewallModFail,
            invalidInstall,
            storeNotRunning,
            connectionFailClient,
            invalidPacket,
            outboundConnectionAlreadyOpen,
            versionMismatch,
            remotePlayerErr,
            syncFail,
            dependencyFail,
            subnauticaError,
            lockErr,
            invalidFunctionCall,
            onedriveFolderDetected
        }
        public static bool DisplayStatusCode(StatusCode statusCode, bool fatal)
        {
            // If statusCode reported is the crash code for piracy,
            if(statusCode == StatusCode.deadPiratesTellNoTales)
            {
                // arrrrr, all pirates must complete the cool challenge in the discord(aka get banned) ;)
                CustomMessageBox customMessage = new(statusCode, true);
                customMessage.StartPosition = FormStartPosition.CenterParent;
                customMessage.ShowDialog();
            // if it's any other status code
            } else
            {
                // Display a popup message box using CustomMessageBox.cs which has most of the buttons and strings filled in with a placeholder for the statusCode
                CustomMessageBox customMessage = new(statusCode, false);
                customMessage.StartPosition = FormStartPosition.CenterParent;
                customMessage.ShowDialog();
            }
            if (fatal)
            {
                Application.Exit();
            }
            return true;
        }
        // Print the statusCode to the server console(only for statusCodes that are due to a server-side crash)
        public static bool PrintStatusCode(StatusCode statusCode, bool fatal)
        {
            if (statusCode != StatusCode.success)
            {
                // ToString("D") prints the integer value of the statusCode enum
                Log.Error(string.Concat("Status code = ", statusCode.ToString("D"), " <- Look up this code on the nitrox website for more information about this error"));
            }
            else
            {
                Log.Info(string.Concat("Status code = ", statusCode.ToString("D")));
            }
            if (fatal)
            {
                Application.Exit();
            }
            return true;
        }
    }
}
