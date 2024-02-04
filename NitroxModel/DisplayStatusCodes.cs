using System.Windows.Forms;
using static NitroxServer.Server;
namespace NitroxModel
{
    public class DisplayStatusCodes
    {
        public static void DisplayStatusCode(StatusCode statusCode)
        {
            CustomMessageBox customMessage = new CustomMessageBox(statusCode);
            customMessage.StartPosition = FormStartPosition.CenterParent;
            customMessage.ShowDialog();

        }
    }
}
