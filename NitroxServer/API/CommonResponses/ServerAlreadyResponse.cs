using System;

namespace NitroxServer.PublicAPI.Routes
{
    [Serializable]
    public class ServerAlreadyResponse
    {
        public string Status;
        public ServerAlreadyResponse(string Status) { this.Status = Status; }
    }
}

