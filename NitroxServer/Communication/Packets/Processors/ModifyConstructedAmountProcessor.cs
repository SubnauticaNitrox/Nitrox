using NitroxModel.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors;

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
