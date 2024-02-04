using System.Windows.Forms;
using static NitroxServer.Server;
namespace NitroxModel
{
    public class DisplayStatusCodes
    {
        public static void DisplayStatusCode(StatusCode statusCode)
        {
            // Display the error message through a popup
            if (statusCode != StatusCode.zero)
            {
                MessageBox.Show(
    "Nitrox has encountered an error!",
    "Nitrox has run into an error with the status code " + statusCode.ToString("D") + "! Look up this status code on the nitrox website(https://www.nitrox.rux.gg/) using the help button below for more information.",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Information,
    MessageBoxDefaultButton.Button1,
    0,
    "https://www.nitrox.rux.gg/",
    "Help");
            }

        }
    }
}
