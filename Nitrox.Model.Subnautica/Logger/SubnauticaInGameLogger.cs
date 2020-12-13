using Nitrox.Model.Logger;

namespace Nitrox.Model.Subnautica.Logger
{
    public class SubnauticaInGameLogger : InGameLogger
    {
        public void Log(object message) => Log(message?.ToString());
        public void Log(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            ErrorMessage.AddMessage(message);
        }
    }
}
