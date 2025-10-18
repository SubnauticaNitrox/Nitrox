using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.MultiplayerSession
{
    [Serializable]
    public class AuthenticationContext
    {
        public string Username { get; }
        public Optional<string> ServerPassword { get; }

        public AuthenticationContext(string username, Optional<string> serverPassword)
        {
            Username = username;
            ServerPassword = serverPassword;
        }
    }
}
