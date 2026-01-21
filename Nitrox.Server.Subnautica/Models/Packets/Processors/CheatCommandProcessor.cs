using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class CheatCommandProcessor(ILogger<CheatCommandProcessor> logger) : IAuthPacketProcessor<CheatCommand>
{
    public async Task Process(AuthProcessorContext context, CheatCommand packet)
    {
        if (context.Sender.Permissions < Perms.MODERATOR)
        {
            logger.ZLogWarning($"{context.Sender.Name} used cheat command: '{packet.Command}' without sufficient permissions.");
            return;
        }

        logger.ZLogInformation($"{context.Sender.Name} used cheat command: '{packet.Command}'");
    }
}
