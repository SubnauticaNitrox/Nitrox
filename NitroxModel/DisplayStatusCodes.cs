using System.Windows.Forms;
using System;
namespace NitroxModel
{
    public class DisplayStatusCodes
    {
        // List all possible status codes, might be a better way to do something repetive like this(could add more descriptive names too)
        public enum StatusCode
        {
            success = 200,
            portNotListening = 504,
            miscUnhandledException = 500,
            privilegesErr = 403,
            fileSystemErr = 507,
            missingFeature = 501,
            deadPiratesTellNoTales = 451,
            invalidVariableVal = 422,
            internetConnectFailLauncher = 408,
            injectionFail = 409,
            firewallModFail = 502,
            invalidInstall = 428,
            storeNotRunning = 412,
            connectionFailClient = 404,
            invalidPacket = 400,
            outboundConnectionAlreadyOpen = 429,
            versionMismatch = 426,
            remotePlayerErr = 401,
            syncFail = 500,
            dependencyFail = 424,
            subnauticaError = 410,
            lockErr = 503,
            invalidFunctionCall = 405,
        }
        public static bool DisplayStatusCode(StatusCode statusCode, bool fatal, string exception)
        {
            // Display a popup message box using CustomMessageBox.cs which has most of the buttons and strings filled in with a placeholder for the statusCode
            CustomMessageBox customMessage = new(statusCode, exception);
            customMessage.StartPosition = FormStartPosition.CenterParent;
            customMessage.ShowDialog();
            // If the error is fatal, exit nitrox
            if (fatal)
            {
                Environment.Exit(1);
            }
            return true;
        }
        // Print the statusCode to the server console(only for statusCodes that are due to a server-side crash)
        public static bool PrintStatusCode(StatusCode statusCode, bool fatal, string exception)
        {
            // ToString("D") prints the integer value of the statusCode enum
            Log.Error(string.Concat("Status code = ", statusCode.ToString("D"), " <- Look up this code on the nitrox website for more information about this error." + "Exception message: " + exception));
            // If the error is fatal, exit nitrox
            if (fatal)
            {
                Environment.Exit(1);
            }
            return true;
        }
    }
}
