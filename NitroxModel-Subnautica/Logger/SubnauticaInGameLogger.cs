using NitroxModel.Logger;

namespace NitroxModel_Subnautica.Logger
{
    public class SubnauticaInGameLogger : InGameLogger
    {
        public void Log(string text)
        {
            ErrorMessage.AddMessage(text);
        }
    }
}
