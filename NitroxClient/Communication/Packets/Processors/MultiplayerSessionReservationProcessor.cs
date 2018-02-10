using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.Communication.Packets.Processors.Abstract;
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
            _multiplayerSession.ProcessReservationResponsePacket(packet);
        }
    }
}
