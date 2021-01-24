using System;

namespace NitroxClient.Persistence.Model
{
    [Serializable]
    public class SavedServer
    {
        public string Name;

        public string Ip;

        public string Port;

        public string Token;
    }
}
