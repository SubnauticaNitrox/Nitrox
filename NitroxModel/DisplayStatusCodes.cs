using System.Windows.Forms;
using System;
using System.Diagnostics;
using NitroxModel.Helper;
using System.Diagnostics.Eventing.Reader;
namespace NitroxModel
{
    public class DisplayStatusCodes
    {
        // All possible status codes, uses HTTP error codes
        public enum StatusCode
        {
            SUCCESS = 200,
            PORT_NOT_LISTENING = 504,
            MISC_UNHANDLED_EXCEPTION = 500,
            PRIVILEGES_ERR = 403,
            FILE_SYSTEM_ERR = 507,
            MISSING_FEATURE = 501,
            DEAD_PIRATES_TELL_NO_TALES = 451,
            INVALID_VARIABLE_VAL = 422,
            INTERNET_CONNECTION_FAIL_LAUNCHER = 408,
            INJECTION_FAIL = 409,
            FIREWALL_MOD_FAIL = 502,
            INVALID_INSTALL = 428,
            STORE_NOT_RUNNING = 412,
            CONNECTION_FAIL_CLIENT = 404,
            INVALID_PACKET = 400,
            OUTBOUND_CONNECTION_ALREADY_OPEN = 429,
            VERSION_MISMATCH = 426,
            REMOTE_PLAYER_ERR = 401,
            SYNC_FAIL = 503,
            DEPENDENCY_FAIL = 424,
            SUBNAUTICA_ERROR = 410,
            LOCK_ERR = 503,
            INVALID_FUNCTION_CALL = 405
        }
        // Display a statusCode to the user(should be for statusCodes that are due to an error from the client)
        public static void DisplayStatusCode(StatusCode statusCode, string exception)
        {
            // Only make a popup message if the code is important, and most likely standalone
            if (statusCode == StatusCode.PORT_NOT_LISTENING || statusCode == StatusCode.PRIVILEGES_ERR || statusCode == StatusCode.FILE_SYSTEM_ERR || statusCode == StatusCode.DEAD_PIRATES_TELL_NO_TALES || statusCode == StatusCode.DEPENDENCY_FAIL
                || statusCode == StatusCode.VERSION_MISMATCH || statusCode == StatusCode.INVALID_INSTALL || statusCode == StatusCode.STORE_NOT_RUNNING || statusCode == StatusCode.FIREWALL_MOD_FAIL || statusCode == StatusCode.INTERNET_CONNECTION_FAIL_LAUNCHER)
            {
                CustomMessageBox customMessage = new(statusCode, exception);
                customMessage.StartPosition = FormStartPosition.CenterParent;
                customMessage.ShowDialog();
            }
            else if (NitroxEnvironment.ReleasePhase == "InDev")
            {
                // Only log on in game on InDev sessions, average player cannot interpret the message and it would simply be extra clutter
                Log.InGame("Error " + statusCode.ToString("D") + ": " + exception);
            }
        }

        // Print the statusCode to the server console(only for statusCodes that are due to an error from the server)
        public static void PrintStatusCode(StatusCode statusCode, string exception)
        {
            // If the status code is important, log it in the server console
            if (statusCode == StatusCode.PORT_NOT_LISTENING || statusCode == StatusCode.PRIVILEGES_ERR || statusCode == StatusCode.FILE_SYSTEM_ERR || statusCode == StatusCode.DEAD_PIRATES_TELL_NO_TALES || statusCode == StatusCode.DEPENDENCY_FAIL
    || statusCode == StatusCode.VERSION_MISMATCH || statusCode == StatusCode.INVALID_INSTALL || statusCode == StatusCode.STORE_NOT_RUNNING || statusCode == StatusCode.FIREWALL_MOD_FAIL || statusCode == StatusCode.INTERNET_CONNECTION_FAIL_LAUNCHER)
            {
                Log.Error(string.Concat("Status code = ", statusCode.ToString("D"), " <- Look up this code on the nitrox website for more information about this error." + "Exception message: " + exception));
            }
            else if (NitroxEnvironment.ReleasePhase == "InDev")
            {
                Log.Error(string.Concat("Status code = ", statusCode.ToString("D"), " <- Look up this code on the nitrox website for more information about this error." + "Exception message: " + exception));
            }
        }
    }
}
