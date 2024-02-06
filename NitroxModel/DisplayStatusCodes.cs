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
            twelve,
            thirteen,
            fourteen,
            fifteen,
            sixteen,
            seventeen,
            eighteen,
            nineteen,
            twenty,
            twentyone,
            twentytwo
        }
        public static void DisplayStatusCode(StatusCode statusCode)
        {
            if(statusCode == StatusCode.nine)
            {
                // arrrrr, all pirates must complete the cool challenge in the discord ;)
                CustomMessageBox customMessage = new(statusCode, true);
                customMessage.StartPosition = FormStartPosition.CenterParent;
                customMessage.ShowDialog();
            } else
            {
                CustomMessageBox customMessage = new(statusCode, false);
                customMessage.StartPosition = FormStartPosition.CenterParent;
                customMessage.ShowDialog();
            }

        }
    }
}
