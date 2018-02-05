using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class MultiplayerSessionReservationProcessor : ClientPacketProcessor<MultiplayerSessionReservation>
    {
        private readonly MultiplayerSessionManager _multiplayerSession;

        public MultiplayerSessionReservationProcessor(MultiplayerSessionManager multiplayerSession)
        {
            _multiplayerSession = multiplayerSession;
        }

        public override void Process(MultiplayerSessionReservation packet)
        {
            if (packet.ReservationState == MultiplayerSessionReservationState.Reserved)
            {
                _multiplayerSession.ConfirmReservation(packet.CorrelationId, packet.ReservationKey);
            }
            else
            {
                _multiplayerSession.HandleRejectedReservation(packet.CorrelationId, packet.ReservationState);
            }
        }
    }
}
