using System;
using NitroxModel.MultiplayerSession;

namespace NitroxModel.Packets
{
    [Serializable]
    public class MultiplayerSessionPolicy : CorrelatedPacket
    {
        public bool RequiresServerPassword { get; }
        public bool DisableConsole { get; }

        public MultiplayerSessionAuthenticationAuthority AuthenticationAuthority { get; }

        public string NitroxVersionAllowed { get; }

        public MultiplayerSessionPolicy(string correlationId, bool disableConsole)
            : base(correlationId)
        {
            // This is done intentionally. It is only a stub for future extension.
            RequiresServerPassword = false;
            AuthenticationAuthority = MultiplayerSessionAuthenticationAuthority.Server;
            DisableConsole = disableConsole;
            NitroxVersionAllowed = typeof(MultiplayerSessionPolicy).Assembly.FullName;
        }
    }
}
