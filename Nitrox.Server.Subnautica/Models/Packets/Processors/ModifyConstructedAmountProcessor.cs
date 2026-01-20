using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class ModifyConstructedAmountProcessor : BuildingProcessor<ModifyConstructedAmount>
{
    private readonly IPacketSender packetSender;
    public ModifyConstructedAmountProcessor(BuildingManager buildingManager, IPacketSender packetSender) : base(buildingManager, packetSender) {
        this.packetSender = packetSender;
    }

    public override void Process(ModifyConstructedAmount packet, Player player)
    {
        if (BuildingManager.ModifyConstructedAmount(packet))
        {
            packetSender.SendPacketToOthersAsync(packet, player.SessionId);
        }
    }
}
