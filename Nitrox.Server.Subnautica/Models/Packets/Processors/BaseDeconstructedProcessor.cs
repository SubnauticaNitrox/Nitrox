using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class BaseDeconstructedProcessor(BuildingManager buildingManager, IPacketSender packetSender) : BuildingProcessor<BaseDeconstructed>(buildingManager, packetSender)
{
    private readonly IPacketSender packetSender = packetSender;

    public override void Process(BaseDeconstructed packet, Player player)
    {
        if (BuildingManager.ReplaceBaseByGhost(packet))
        {
            packetSender.SendPacketToOthersAsync(packet, player.SessionId);
        }
    }
}
