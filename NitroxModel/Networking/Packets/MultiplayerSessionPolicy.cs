using System;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Networking.Session;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public record MultiplayerSessionPolicy : CorrelatedPacket    {
        public bool RequiresServerPassword { get; }
        public MultiplayerSessionAuthenticationAuthority AuthenticationAuthority { get; }
        public bool DisableConsole { get; }
        public int MaxConnections { get; }
        public NitroxVersion NitroxVersionAllowed { get; }

        public MultiplayerSessionPolicy(string correlationId, bool disableConsole, int maxConnections, bool requiresServerPassword) : base(correlationId)
        {
            RequiresServerPassword = requiresServerPassword;
            AuthenticationAuthority = MultiplayerSessionAuthenticationAuthority.SERVER;
            DisableConsole = disableConsole;
            MaxConnections = maxConnections;

            Version ver = NitroxEnvironment.Version;
            // only the major and minor version number is required
            NitroxVersionAllowed = new(ver.Major, ver.Minor);
        }

        /// <remarks>Used for deserialization</remarks>
        public MultiplayerSessionPolicy(string correlationId, bool requiresServerPassword, MultiplayerSessionAuthenticationAuthority authenticationAuthority,
                                        bool disableConsole, int maxConnections, NitroxVersion nitroxVersionAllowed) : base(correlationId)
        {
            RequiresServerPassword = requiresServerPassword;
            AuthenticationAuthority = authenticationAuthority;
            DisableConsole = disableConsole;
            MaxConnections = maxConnections;
            NitroxVersionAllowed = nitroxVersionAllowed;
        }
    }
}
