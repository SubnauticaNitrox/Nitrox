using System;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.MultiplayerSession
{
    [Serializable]
    public class AuthenticationContext
    {
        public Guid AuthToken { get; }

        public String Username { get; }
        public Optional<string> ServerPassword { get; }

        public AuthenticationContext(Guid authToken, string username) : this(authToken, username, null)
        {
        }

        public AuthenticationContext(Guid authToken, string username, string serverPassword)
        {
            Username = username;
            AuthToken = authToken;
            ServerPassword = Optional.OfNullable(serverPassword);
        }
    }
}
