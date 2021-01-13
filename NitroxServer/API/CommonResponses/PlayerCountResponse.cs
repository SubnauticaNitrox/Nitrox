using System;

namespace NitroxServer.PublicAPI.Routes
{
    [Serializable]
    public class PlayerCountResponse
    {
        public int Count;
        public int Max;
    }
}

