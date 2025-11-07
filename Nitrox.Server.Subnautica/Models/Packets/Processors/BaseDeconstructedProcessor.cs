using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class BaseDeconstructedProcessor : BuildingProcessor<BaseDeconstructed>
{
    public BaseDeconstructedProcessor(BuildingManager buildingManager, PlayerManager playerManager) : base(buildingManager, playerManager) { }

    public override void Process(BaseDeconstructed packet, Player player)
    {
        if (buildingManager.ReplaceBaseByGhost(packet))
        {
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
