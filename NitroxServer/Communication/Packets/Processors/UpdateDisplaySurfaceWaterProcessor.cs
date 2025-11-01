using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;

namespace NitroxServer.Communication.Packets.Processors;

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
