using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlaceGhostProcessor(BuildingManager buildingManager, IPacketSender packetSender) : BuildingProcessor<PlaceGhost>(buildingManager, packetSender)
{
    public override void Process(PlaceGhost packet, Player player)
    {
        if (BuildingManager.AddGhost(packet))
        {
            PacketSender.SendPacketToOthersAsync(packet, player.SessionId);
        }
    }
}
