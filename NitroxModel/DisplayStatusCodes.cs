using System.Windows.Forms;
using System;
using System.Diagnostics;
namespace NitroxModel
{
    public class DisplayStatusCodes
    {
        
        // Counter variables to check if the same code is happening over and over again to mark it as fatal
        private static int repeatCodeCountDisplay = 0;
        private static StatusCode lastCodeDisplay = StatusCode.SUCCESS;
        private static int repeatCodeCountPrint = 0;
        private static StatusCode lastCodePrint = StatusCode.SUCCESS;
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
        public static void DisplayStatusCode(StatusCode statusCode, bool fatal, string exception)
        {
                // If the statusCode is the same as the last one we displayed, increase the repeated codes counter
                if (statusCode == lastCodeDisplay && statusCode != StatusCode.CONNECTION_FAIL_CLIENT)
                {
                    repeatCodeCountDisplay++;
                }
                // If the statusCode is not the same, reset the repeated codes counter
                else
                {
                    repeatCodeCountDisplay = 0;
                }
                // If the same code is repeated 3 times in a row(including this code), then the error is fatal
                if (repeatCodeCountDisplay == 2)
                {
                    fatal = true;
                }
                // Set the last statusCode variable for the next time the function runs
                lastCodeDisplay = statusCode;
                // Display a popup message box using CustomMessageBox.cs which has most of the buttons and strings filled in with a placeholder for the statusCode
                CustomMessageBox customMessage = new(statusCode, exception);
                customMessage.StartPosition = FormStartPosition.CenterParent;
                customMessage.ShowDialog();
                // If the error is fatal, exit nitrox
                if (fatal)
                {
                    // Environment.Exit(1);
                }
                // If the error is not fatal, continue running
        }

        // Print the statusCode to the server console(only for statusCodes that are due to a server-side crash)
        public static void PrintStatusCode(StatusCode statusCode, bool fatal, string exception)
        {
            // If the statusCode is the same as the last one we printed, increase the repeated codes counter
            if(statusCode == lastCodePrint && statusCode != StatusCode.CONNECTION_FAIL_CLIENT)
            {
                repeatCodeCountPrint++;
            }
            // If the statusCode is different, reset the counter
            else
            {
                repeatCodeCountPrint = 0;
            }
            // If the same code happens 3 times in a row, then the error is fatal and we will exit nitrox
            if(repeatCodeCountPrint == 2)
            {
                fatal = true;
            }
            // Set the last code printed variable for the next time the function runs
            lastCodePrint = statusCode;
            // Log the status code to console along with the exception message
            Log.Error(string.Concat("Status code = ", statusCode.ToString("D"), " <- Look up this code on the nitrox website for more information about this error." + "Exception message: " + exception));
            // If the error is fatal, exit nitrox
            if (fatal)
            {
                Environment.Exit(1);
            }
            // If the error is not fatal, continue running
        }
    }
}
