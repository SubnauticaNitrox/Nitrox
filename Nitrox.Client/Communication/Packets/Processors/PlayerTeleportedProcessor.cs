using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;

namespace Nitrox.Client.Communication.Packets.Processors
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
