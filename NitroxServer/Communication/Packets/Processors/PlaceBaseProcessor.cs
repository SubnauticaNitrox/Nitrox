using NitroxModel.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors;

public class PlaceBaseProcessor : BuildingProcessor<PlaceBase>
{
    public PlaceBaseProcessor(BuildingManager buildingManager, PlayerManager playerManager) : base(buildingManager, playerManager) { }

    public override void Process(PlaceBase packet, Player player)
    {
        if (buildingManager.CreateBase(packet))
        {
            packet.BuildEntity = null;
            base.Process(packet, player);
        }
    }
}
