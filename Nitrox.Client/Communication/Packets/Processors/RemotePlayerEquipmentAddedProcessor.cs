using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;

namespace Nitrox.Client.Communication.Packets.Processors
{
    public class RemotePlayerEquipmentAddedProcessor : ClientPacketProcessor<RemotePlayerEquipmentAdded>
    {
        private readonly PlayerManager playerManager;

        public RemotePlayerEquipmentAddedProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(RemotePlayerEquipmentAdded packet)
        {
            ushort playerId = packet.PlayerId;

            RemotePlayer player;
            if (!playerManager.TryFind(playerId, out player))
            {
                return;
            }

            TechType techType = packet.TechType.ToUnity();
            player.AddEquipment(techType);
        }
    }
}
