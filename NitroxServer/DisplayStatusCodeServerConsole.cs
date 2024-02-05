namespace NitroxServer
{
    public class DisplayStatusCodeServerConsole
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
        public static void PrintStatusCode(StatusCode statusCode)
        {
            if (statusCode != StatusCode.zero)
            {
                // ToString("D") prints the integer value of the statusCode enum
                Log.Error(string.Concat("Status code = ", statusCode.ToString("D"), " <- Look up this code on the nitrox website for more information about this error"));
            }
            else
            {
                Log.Info(string.Concat("Status code = ", statusCode.ToString("D")));
            }
        }
    }
}
