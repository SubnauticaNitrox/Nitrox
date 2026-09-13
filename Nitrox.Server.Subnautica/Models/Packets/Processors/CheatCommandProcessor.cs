using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class CheatCommandProcessor(PlayerService playerService, ILogger<CheatCommandProcessor> logger) : IAuthPacketProcessor<CheatCommand>
{
    private readonly PlayerService playerService = playerService;
    private readonly ILogger<CheatCommandProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, CheatCommand packet)
    {
        if (playerService.GetProperty<PermissionsProperty>(context.Sender).Value < Perms.MODERATOR)
        {
            logger.ZLogWarning($"{playerService.GetProperty<NameProperty>(context.Sender).Value} #{context.Sender} used cheat command: '{packet.Command}' without sufficient permissions.");
            return;
        }

        logger.ZLogInformation($"{playerService.GetProperty<NameProperty>(context.Sender).Value} #{context.Sender} used cheat command: '{packet.Command}'");
    }
}
