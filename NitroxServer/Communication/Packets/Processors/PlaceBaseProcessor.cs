using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class PlaceBaseProcessor : BuildingProcessor<PlaceBase>
{
    public PlaceBaseProcessor(BuildingManager buildingManager, PlayerManager playerManager, EntitySimulation entitySimulation) : base(buildingManager, playerManager, entitySimulation){ }

    public override void Process(PlaceBase packet, Player player)
    {
        if (buildingManager.CreateBase(packet))
        {
            BuildEntity buildEntity = packet.BuildEntity;
            ClaimBuildPiece(buildEntity, player);
            
            // End-players can process elementary operations without this data (packet would be heavier for no reason)
            packet.Deflate();
            playerManager.SendPacketToOtherPlayers(packet, player);

            // Need to be spawned separately since other players will not have a MoonpoolManager or moonpool ids
            // Maybe this also needs to be done for other types of base pieces?
            List<Entity> moonpools = buildEntity.ChildEntities.Where(entity => entity is MoonpoolEntity).ToList();
            if (moonpools.Count > 0)
            {
                playerManager.SendPacketToOtherPlayers(new SpawnEntities(moonpools), player);
            }
        }
    }
}
