using System;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.MultiplayerSession
{
    [Serializable]
    public class AuthenticationContext
    {
        public string Username { get; }
        public Optional<string> ServerPassword { get; }

        public AuthenticationContext(string username)
        {
            Username = username;
        }

        public AuthenticationContext(string username, string serverPassword) 
            : this(username)
        {
            ServerPassword = Optional<string>.Of(serverPassword);
        }
    }
}
