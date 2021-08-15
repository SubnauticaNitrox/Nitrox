using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.Communication.Packets.Processors
{
    public class RemotePlayerEquipmentRemovedProcessor : ClientPacketProcessor<RemotePlayerEquipmentRemoved>
    {
        private readonly PlayerManager playerManager;

        public RemotePlayerEquipmentRemovedProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(RemotePlayerEquipmentRemoved packet)
        {
            ushort playerId = packet.PlayerId;

            if (!playerManager.TryFind(playerId, out RemotePlayer player))
            {
                return;
            }

            TechType techType = packet.TechType.ToUnity();
            player.RemoveEquipment(techType);
        }
    }
}
