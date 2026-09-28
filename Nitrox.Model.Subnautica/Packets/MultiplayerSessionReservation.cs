using System;
using Nitrox.Model.Core;
using Nitrox.Model.MultiplayerSession;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class MultiplayerSessionReservation(
    SessionId sessionId,
    MultiplayerSessionReservationState reservationState = MultiplayerSessionReservationState.RESERVED,
    string? rejectionReason = null)
    : Packet
{
    /// <summary>
    ///     Gets the session id of the player.
    /// </summary>
    public SessionId SessionId { get; } = sessionId;

    public MultiplayerSessionReservationState ReservationState { get; } = reservationState;

    /// <summary>
    ///     Optional human-readable detail shown to the player on top of the generic <see cref="ReservationState" />
    ///     description, e.g. the reason and expiry of a ban. <see langword="null" /> when there is nothing to add.
    /// </summary>
    public string? RejectionReason { get; } = rejectionReason;
}
