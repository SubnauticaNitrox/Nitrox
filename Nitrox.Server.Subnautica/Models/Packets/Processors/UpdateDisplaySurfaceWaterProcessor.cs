using Nitrox.Model.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

/// <summary>
/// Stores the state of a player displaying surface water
/// </summary>
public class UpdateDisplaySurfaceWaterProcessor : AuthenticatedPacketProcessor<UpdateDisplaySurfaceWater>
{
    public override void Process(UpdateDisplaySurfaceWater packet, Player player)
    {
        player.DisplaySurfaceWater = packet.DisplaySurfaceWater;
    }
}
