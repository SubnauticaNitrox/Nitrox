using System;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.MultiplayerSession
{
    [Serializable]
    public class AuthenticationContext
    {
        public string Username { get; }
        public ulong SteamID { get; }
        public Optional<string> ServerPassword { get; }

        public AuthenticationContext(string username)
        {
            Username = username;
        }

        public AuthenticationContext(string username, ulong steamID)
        {
            Username = username;
            SteamID = steamID;
        }

        public AuthenticationContext(string username, string serverPassword, ulong steamID)
            : this(username)
        {
            ServerPassword = Optional<string>.Of(serverPassword);
            SteamID = steamID;
        }
    }
}
