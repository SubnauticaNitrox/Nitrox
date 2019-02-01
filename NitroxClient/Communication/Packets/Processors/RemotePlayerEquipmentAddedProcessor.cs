using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
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

            TechType techType = packet.TechType;
            player.AddEquipment(techType);
        }
    }
}
