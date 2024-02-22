using System.Windows.Forms;
using System;
namespace NitroxModel
{
    public class DisplayStatusCodes
    {
        // List all possible status codes, might be a better way to do something repetive like this(could add more descriptive names too)
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
