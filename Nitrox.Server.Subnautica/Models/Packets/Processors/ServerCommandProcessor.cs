using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Dto;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class ServerCommandProcessor(CommandService commandService, PlayerRepository playerRepository, IServerPacketSender packetSender, ILogger<ServerCommandProcessor> logger) : IAuthPacketProcessor<ServerCommand>
{
    private readonly CommandService commandService = commandService;
    private readonly PlayerRepository playerRepository = playerRepository;
    private readonly IServerPacketSender packetSender = packetSender;
    private readonly ILogger<ServerCommandProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, ServerCommand packet)
    {
        ConnectedPlayerDto player = await playerRepository.GetConnectedPlayerBySessionIdAsync(context.Sender.SessionId);
        if (player == null)
        {
            return;
        }
        logger.LogInformation("{PlayerName} issued command: /{Command}", player.Name, packet.Cmd);
        commandService.ExecuteCommand(packet.Cmd, new PlayerToServerCommandContext(packetSender, player));
    }
}
