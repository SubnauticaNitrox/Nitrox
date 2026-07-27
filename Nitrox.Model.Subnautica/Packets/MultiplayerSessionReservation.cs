using System;
using Nitrox.Model.Core;
using Nitrox.Model.MultiplayerSession;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class MultiplayerSessionReservation(
    SessionId sessionId,
    MultiplayerSessionReservationState reservationState = MultiplayerSessionReservationState.RESERVED)
    : Packet
{
    /// <summary>
    ///     Gets the session id of the player.
    /// </summary>
    public SessionId SessionId { get; } = sessionId;

    public MultiplayerSessionReservationState ReservationState { get; } = reservationState;
}
