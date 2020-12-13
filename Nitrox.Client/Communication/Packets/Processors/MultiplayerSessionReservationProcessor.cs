using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
{
    public class MultiplayerSessionReservationProcessor : ClientPacketProcessor<MultiplayerSessionReservation>
    {
        private readonly IMultiplayerSession multiplayerSession;

        public MultiplayerSessionReservationProcessor(IMultiplayerSession multiplayerSession)
        {
            this.multiplayerSession = multiplayerSession;
        }

        public override void Process(MultiplayerSessionReservation packet)
        {
            multiplayerSession.ProcessReservationResponsePacket(packet);
        }
    }
}
