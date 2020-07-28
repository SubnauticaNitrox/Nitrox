using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlayerTeleportedProcessor : ClientPacketProcessor<PlayerTeleported>
    {
        public PlayerTeleportedProcessor()
        {
        }

        public override void Process(PlayerTeleported packet)
        {
            Player.main.SetPosition(packet.DestinationTo.ToUnity());
            Player.main.OnPlayerPositionCheat();
        }
    }
}
