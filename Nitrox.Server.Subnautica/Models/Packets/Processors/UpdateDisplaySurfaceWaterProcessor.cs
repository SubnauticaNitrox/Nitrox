using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

/// <summary>
/// Stores the state of a player displaying surface water
/// </summary>
internal sealed class UpdateDisplaySurfaceWaterProcessor(PlayerService playerService) : IAuthPacketProcessor<UpdateDisplaySurfaceWater>
{
    private readonly PlayerService playerService = playerService;

    public Task Process(AuthProcessorContext context, UpdateDisplaySurfaceWater packet)
    {
        playerService.GetProperty<DisplaySurfaceWaterProperty>(context.Sender).Value = packet.DisplaySurfaceWater;
        return Task.CompletedTask;
    }
}
