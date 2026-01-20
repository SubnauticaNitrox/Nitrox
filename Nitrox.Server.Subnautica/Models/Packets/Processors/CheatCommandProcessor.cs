using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class CheatCommandProcessor(ILogger<CheatCommandProcessor> logger) : AuthenticatedPacketProcessor<CheatCommand>
{
    public override void Process(CheatCommand packet, Player player)
    {
        if (player.Permissions < Perms.MODERATOR)
        {
            logger.ZLogWarning($"{player.Name} used cheat command: '{packet.Command}' without sufficient permissions.");
            return;
        }

        logger.ZLogInformation($"{player.Name} used cheat command: '{packet.Command}'");
    }
}
