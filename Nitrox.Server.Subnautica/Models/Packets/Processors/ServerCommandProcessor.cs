using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class ServerCommandProcessor(CommandService cmdProcessor, IPacketSender packetSender, ILogger<ServerCommandProcessor> logger) : AuthenticatedPacketProcessor<ServerCommand>
{
    private readonly CommandService cmdProcessor = cmdProcessor;
    private readonly ILogger<ServerCommandProcessor> logger = logger;
    private readonly IPacketSender packetSender = packetSender;

    public override void Process(ServerCommand packet, Player player)
    {
        logger.ZLogInformation($"{player.Name} issued command '/{packet.Cmd}'");
        cmdProcessor.ExecuteCommand(packet.Cmd, new PlayerToServerCommandContext(packetSender, player));
    }
}
