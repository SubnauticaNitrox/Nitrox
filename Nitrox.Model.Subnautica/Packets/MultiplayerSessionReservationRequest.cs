using System;
using Nitrox.Model.MultiplayerSession;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.MultiplayerSession;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class MultiplayerSessionReservationRequest(PlayerSettings playerSettings, AuthenticationContext authenticationContext) : Packet
{
    public PlayerSettings PlayerSettings { get; } = playerSettings;
    public AuthenticationContext AuthenticationContext { get; } = authenticationContext;
}
