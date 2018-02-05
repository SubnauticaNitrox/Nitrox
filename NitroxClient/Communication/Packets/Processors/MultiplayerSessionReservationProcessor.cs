using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class MultiplayerSessionReservationProcessor : ClientPacketProcessor<MultiplaySessionReservation>
    {
        private readonly MultiplayerSessionManager _multiplayerSessionManager;

        public MultiplayerSessionReservationProcessor(MultiplayerSessionManager multiplayerSessionManager)
        {
            _multiplayerSessionManager = multiplayerSessionManager;
        }

        public override void Process(MultiplaySessionReservation packet)
        {
            if (packet.ReservationState == MultiplayerSessionReservationState.Reserved)
            {
                _multiplayerSessionManager.ConfirmReservation(packet.CorrelationId, packet.ReservationKey);
            }
            else
            {
                _multiplayerSessionManager.HandleRejectedReservation(packet.CorrelationId, packet.ReservationState);
            }
        }
    }
}
