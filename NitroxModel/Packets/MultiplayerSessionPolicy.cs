using System;
using NitroxModel.MultiplayerSession;

namespace NitroxModel.Packets
{
    [Serializable]
    public class MultiplayerSessionPolicy : Packet
    {
        public bool RequiresServerPassword { get; }

        public MultiplayerSessionAuthenticationAuthority AuthenticationAuthority { get; }

        public MultiplayerSessionPolicy()
        {
            //This is done intentionally. This is only a stub for future extension.
            RequiresServerPassword = false;
            AuthenticationAuthority = MultiplayerSessionAuthenticationAuthority.Server;
        }
    }
}
