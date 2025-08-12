using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    public class PlayerJoiningMultiplayerSessionProcessor : UnauthenticatedPacketProcessor<PlayerJoiningMultiplayerSession>
    {
        private readonly JoiningManager joiningManager;

        public PlayerJoiningMultiplayerSessionProcessor(JoiningManager joiningManager)
        {
            this.joiningManager = joiningManager;
        }

        public override void Process(PlayerJoiningMultiplayerSession packet, INitroxConnection connection)
        {
            joiningManager.AddToJoinQueue(connection, packet.ReservationKey);
        }
    }
}
