using System;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.MultiplayerSession
{
    [Serializable]
    public class AuthenticationContext
    {
        public Guid Token { get; }

        public String Username { get; }
        public Optional<string> ServerPassword { get; }

        public AuthenticationContext(Guid token, string username) : this(token, username, null)
        {
        }

        public AuthenticationContext(Guid token, string username, string serverPassword)
        {
            Username = username;
            Token = token;
            ServerPassword = Optional.OfNullable(serverPassword);
        }
    }
}
