using System;

namespace NitroxServer.PublicAPI.Routes
{
    [Serializable]
    public class ServerStatusResponse
    {
        public string Status;
        public int PlayersOnline;
        public int MaxPlayers;
    }
}

