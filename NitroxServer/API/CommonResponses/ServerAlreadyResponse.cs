using System;

namespace NitroxServer.API.CommonResponses
{
    [Serializable]
    public class ServerAlreadyResponse
    {
        public string Status;
        public ServerAlreadyResponse(string Status) { this.Status = Status; }
    }
}

