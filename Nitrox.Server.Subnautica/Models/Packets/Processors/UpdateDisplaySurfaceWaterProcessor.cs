using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

/// <summary>
/// Stores the state of a player displaying surface water
/// </summary>
internal sealed class UpdateDisplaySurfaceWaterProcessor : IAuthPacketProcessor<UpdateDisplaySurfaceWater>
{
    public Task Process(AuthProcessorContext context, UpdateDisplaySurfaceWater packet)
    {
        context.Sender.DisplaySurfaceWater = packet.DisplaySurfaceWater;
        return Task.CompletedTask;
    }
}
