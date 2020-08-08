using System;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.MultiplayerSession
{
    [Serializable]
    public class AuthenticationContext
    {
        public string Username { get; }
        public Optional<string> ServerPassword { get; }

        public AuthenticationContext(string username) : this(username, null)
        {
        }

        public AuthenticationContext(string username, string serverPassword)
        {
            Username = username;
            ServerPassword = Optional.OfNullable(serverPassword);
        }

        public override string ToString()
        {
            return $"[AuthenticationContext - Username: {Username} ServerPassword: {ServerPassword}]";
        }
    }
}
