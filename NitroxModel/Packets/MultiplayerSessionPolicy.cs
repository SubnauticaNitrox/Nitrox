using System;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;

namespace NitroxModel.Packets
{
    [Serializable]
    public class MultiplayerSessionPolicy : CorrelatedPacket
    {
        public bool RequiresServerPassword { get; }
        public bool DisableConsole { get; }
        public int MaxConnections { get; }

        public MultiplayerSessionAuthenticationAuthority AuthenticationAuthority { get; }
        public Version NitroxVersionAllowed { get; }

        public MultiplayerSessionPolicy(string correlationId, bool disableConsole, int maxConnections, bool requiresServerPassword) : base(correlationId)
        {
            RequiresServerPassword = requiresServerPassword;
            AuthenticationAuthority = MultiplayerSessionAuthenticationAuthority.SERVER;
            DisableConsole = disableConsole;
            MaxConnections = maxConnections;

            Version ver = NitroxEnvironment.Version;
            // only the major and minor version number is required
            NitroxVersionAllowed = new Version(ver.Major, ver.Minor);
        }
    }
}
