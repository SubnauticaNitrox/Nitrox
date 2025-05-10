using System;
using NitroxModel.Networking.Session;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public record MultiplayerSessionReservation : CorrelatedPacket    {
        public ushort PlayerId { get; }
        public string ReservationKey { get; }
        public MultiplayerSessionReservationState ReservationState { get; }

        public MultiplayerSessionReservation(string correlationId, MultiplayerSessionReservationState reservationState) : base(correlationId)
        {
            ReservationState = reservationState;
        }
        
        public MultiplayerSessionReservation(string correlationId, ushort playerId, string reservationKey, 
                                             MultiplayerSessionReservationState reservationState = MultiplayerSessionReservationState.RESERVED) : this(correlationId, reservationState)
        {
            PlayerId = playerId;
            ReservationKey = reservationKey;
        }
    }
}
