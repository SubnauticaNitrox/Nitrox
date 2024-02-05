using System.Windows.Forms;
using static NitroxServer.DisplayStatusCodeServerConsole;
namespace NitroxModel
{
    public class DisplayStatusCodes
    {
        public enum StatusCode
        {
            zero,
            one,
            two,
            three,
            four,
            five,
            six,
            seven,
            eight,
            nine,
            ten,
            eleven,
            twelve
        }
        public static void DisplayStatusCode(StatusCode statusCode)
        {
            CustomMessageBox customMessage = new (statusCode);
            customMessage.StartPosition = FormStartPosition.CenterParent;
            customMessage.ShowDialog();

        }
    }
}
