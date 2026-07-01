using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.MultiplayerSession;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class MultiplayerSessionPolicy : Packet
{
    public SessionId SessionId { get; }
    public bool RequiresServerPassword { get; }
    public MultiplayerSessionAuthenticationAuthority AuthenticationAuthority { get; }
    public bool DisableConsole { get; }
    public int MaxConnections { get; }
    public NitroxVersion NitroxVersionAllowed { get; }

    public MultiplayerSessionPolicy(SessionId sessionId, bool disableConsole, int maxConnections, bool requiresServerPassword)
    {
        SessionId = sessionId;
        RequiresServerPassword = requiresServerPassword;
        AuthenticationAuthority = MultiplayerSessionAuthenticationAuthority.SERVER;
        DisableConsole = disableConsole;
        MaxConnections = maxConnections;

        Version ver = NitroxEnvironment.Version;
        // only the major and minor version number is required
        NitroxVersionAllowed = new(ver.Major, ver.Minor);
    }

    /// <remarks>Used for deserialization</remarks>
    public MultiplayerSessionPolicy(SessionId sessionId, bool requiresServerPassword, MultiplayerSessionAuthenticationAuthority authenticationAuthority,
                                    bool disableConsole, int maxConnections, NitroxVersion nitroxVersionAllowed)
    {
        SessionId = sessionId;
        RequiresServerPassword = requiresServerPassword;
        AuthenticationAuthority = authenticationAuthority;
        DisableConsole = disableConsole;
        MaxConnections = maxConnections;
        NitroxVersionAllowed = nitroxVersionAllowed;
    }
}
