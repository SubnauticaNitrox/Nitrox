using Nitrox.Model.Packets;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class ModifyConstructedAmountProcessor : BuildingProcessor<ModifyConstructedAmount>
{
    public ModifyConstructedAmountProcessor(BuildingManager buildingManager, PlayerManager playerManager) : base(buildingManager, playerManager) { }

    public override void Process(ModifyConstructedAmount packet, Player player)
    {
        if (buildingManager.ModifyConstructedAmount(packet))
        {
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
