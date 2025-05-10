using NitroxClient.Communication.Abstract;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class MultiplayerSessionReservationProcessor : IClientPacketProcessor<MultiplayerSessionReservation>
    {
        private readonly IMultiplayerSession multiplayerSession;

        public MultiplayerSessionReservationProcessor(IMultiplayerSession multiplayerSession)
        {
            this.multiplayerSession = multiplayerSession;
        }

        public Task Process(IPacketProcessContext context, MultiplayerSessionReservation packet)
        {
            multiplayerSession.ProcessReservationResponsePacket(packet);

            return Task.CompletedTask;
        }
    }
}
