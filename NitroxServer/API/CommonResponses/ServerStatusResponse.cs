using System;

namespace NitroxServer.API.CommonResponses
{
    [Serializable]
    public class ServerStatusResponse
    {
        public string Status;
        public int PlayersOnline;
        public int MaxPlayers;
    }
}

