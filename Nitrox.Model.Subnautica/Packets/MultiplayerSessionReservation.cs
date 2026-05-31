using System;
using Nitrox.Model.Core;
using Nitrox.Model.MultiplayerSession;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class MultiplayerSessionReservation : CorrelatedPacket
{
    public MultiplayerSessionReservation(string correlationId, MultiplayerSessionReservationState reservationState) : base(correlationId)
    {
        ReservationState = reservationState;
    }

    public MultiplayerSessionReservation(string correlationId, SessionId sessionId, string reservationKey,
                                         MultiplayerSessionReservationState reservationState = MultiplayerSessionReservationState.RESERVED) : this(correlationId, reservationState)
    {
        SessionId = sessionId;
        ReservationKey = reservationKey;
    }

    /// <summary>
    ///     Gets the session id of the player.
    /// </summary>
    public SessionId SessionId { get; }

    public string ReservationKey { get; }
    public MultiplayerSessionReservationState ReservationState { get; }
}
