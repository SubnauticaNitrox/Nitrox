using NitroxModel.DataStructures.Util;
using ZeroFormatter;

namespace NitroxModel.MultiplayerSession
{
    [ZeroFormattable]
    public class AuthenticationContext
    {
        [Index(0)]
        public virtual string Username { get; protected set; }
        [Index(1)]
        public virtual Optional<string> ServerPassword { get; protected set; }

        private AuthenticationContext() { }

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
