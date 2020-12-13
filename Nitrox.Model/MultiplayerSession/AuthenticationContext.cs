using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.MultiplayerSession
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
    }
}
