using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    public class PingRenamedPacketProcessor : AuthenticatedPacketProcessor<PingRenamed>
    {
        private readonly EntityRegistry entityRegistry;
        private readonly PlayerManager playerManager;

        public PingRenamedPacketProcessor(PlayerManager playerManager, EntityRegistry entityRegistry)
        {
            this.playerManager = playerManager;
            this.entityRegistry = entityRegistry;
        }

        public override void Process(PingRenamed packet, Player player)
        {
            Optional<Entity> beaconEntity = entityRegistry.GetEntityById(packet.Id);
            if (!beaconEntity.HasValue)
            {
                Log.Error($"Beacon entity could not be found on server with nitrox id '{packet.Id}'");
                return;
            }

            Log.Info($"Received ping rename: {packet} by player: {player}");
            playerManager.SendPacketToOtherPlayers(packet, player);

            // TODO: this needs to be moved to EntityMetadata
            // Persist label change on server for future players
            //beaconEntity.Value.SerializedGameObject = packet.BeaconGameObjectSerialized;
        }
    }
}
