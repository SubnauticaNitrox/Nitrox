using System;
using NitroxModel.MultiplayerSession;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class MultiplayerSessionPolicy : CorrelatedPacket
    {
        [Index(0)]
        public virtual bool RequiresServerPassword { get; protected set; }
        [Index(1)]
        public virtual bool DisableConsole { get; protected set; }
        [Index(2)]
        public virtual int MaxConnections { get; protected set; }
        [Index(3)]
        public virtual MultiplayerSessionAuthenticationAuthority AuthenticationAuthority { get; protected set; }
        [Index(4)]
        public virtual Version NitroxVersionAllowed { get; protected set; }

        private MultiplayerSessionPolicy() : base(default) { }

        public MultiplayerSessionPolicy(string correlationId, bool disableConsole, int maxConnections, bool requiresServerPassword) : base(correlationId)
        {
            // This is done intentionally. It is only a stub for future extension.
            RequiresServerPassword = requiresServerPassword;
            AuthenticationAuthority = MultiplayerSessionAuthenticationAuthority.SERVER;
            DisableConsole = disableConsole;
            MaxConnections = maxConnections;
            // get the full version name
            Version ver = typeof(MultiplayerSessionPolicy).Assembly.GetName().Version;
            // only the major and minor version number is required
            NitroxVersionAllowed = new Version(ver.Major, ver.Minor);
        }
    }
}
