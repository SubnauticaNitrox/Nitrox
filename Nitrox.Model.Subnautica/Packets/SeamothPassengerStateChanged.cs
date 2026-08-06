using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

/// <summary>
///     Canonical, server-authored passenger state. An empty <see cref="SeamothId" /> means the player is not a passenger.
/// </summary>
[Serializable]
public sealed class SeamothPassengerStateChanged(SessionId sessionId, Optional<NitroxId> seamothId, byte seatIndex, bool accepted) : Packet
{
    public SessionId SessionId { get; } = sessionId;
    public Optional<NitroxId> SeamothId { get; } = seamothId;
    public byte SeatIndex { get; } = seatIndex;
    public bool Accepted { get; } = accepted;
}
