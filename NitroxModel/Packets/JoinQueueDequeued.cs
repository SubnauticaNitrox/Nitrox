namespace NitroxModel.Packets
{
    public class JoinQueueDequeued : CorrelatedPacket
    {
        public ushort PlayerId { get; }
        public string ReservationKey { get; }

        public JoinQueueDequeued(string correlationId, ushort playerId, string reservationKey) : base(correlationId)
        {
            PlayerId = playerId;
            ReservationKey = reservationKey;
        }
    }
}
