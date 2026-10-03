using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

/// <summary>
/// Stores the state of a player being in precursor
/// </summary>
internal sealed class UpdateInPrecursorProcessor(PlayerService playerService) : IAuthPacketProcessor<UpdateInPrecursor>
{
    private readonly PlayerService playerService = playerService;

    public Task Process(AuthProcessorContext context, UpdateInPrecursor packet)
    {
        playerService.GetProperty<InPrecursorProperty>(context.Sender).Value = packet.InPrecursor;
        return Task.CompletedTask;
    }
}
