using System;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.MultiplayerSession
{
    [Serializable]
    public class AuthenticationContext
    {
        public string Username { get; }
        public ulong SteamId { get; }
        public Optional<string> ServerPassword { get; }

        public AuthenticationContext(string username, ulong steamId)
        {
            Username = username;
            SteamId = steamId;
        }

        public AuthenticationContext(string username, ulong steamId, string serverPassword)
            : this(username, steamId)
        {
            ServerPassword = Optional<string>.Of(serverPassword);
        }
    }
}
