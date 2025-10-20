using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.Commands.Processor;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class ServerCommandProcessor(TextCommandProcessor cmdProcessor, ILogger<ServerCommandProcessor> logger) : AuthenticatedPacketProcessor<ServerCommand>
{
    private readonly TextCommandProcessor cmdProcessor = cmdProcessor;
    private readonly ILogger<ServerCommandProcessor> logger = logger;

    public override void Process(ServerCommand packet, Player player)
    {
        logger.ZLogInformation($"{player.Name} issued command '/{packet.Cmd}'");
        cmdProcessor.ProcessCommand(packet.Cmd, Optional.Of(player), player.Permissions);
    }
}
