using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class ServerCommandProcessor(CommandService cmdProcessor, IPacketSender packetSender, ILogger<ServerCommandProcessor> logger) : IAuthPacketProcessor<ServerCommand>
{
    private readonly CommandService cmdProcessor = cmdProcessor;
    private readonly ILogger<ServerCommandProcessor> logger = logger;
    private readonly IPacketSender packetSender = packetSender;

    public Task Process(AuthProcessorContext context, ServerCommand packet)
    {
        logger.ZLogInformation($"{context.Sender.Name} issued command '/{packet.Cmd}'");
        cmdProcessor.ExecuteCommand(packet.Cmd, new PlayerToServerCommandContext(packetSender, context.Sender));
        return Task.CompletedTask;
    }
}
