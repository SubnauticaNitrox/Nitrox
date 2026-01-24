using Nitrox.Model.Core;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Logging.Scopes;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class ServerCommandProcessor(CommandService cmdProcessor, IPacketSender packetSender, ILogger<ServerCommandProcessor> logger) : IAuthPacketProcessor<ServerCommand>
{
    private readonly CommandService cmdProcessor = cmdProcessor;
    private readonly ILogger<ServerCommandProcessor> logger = logger;
    private readonly IPacketSender packetSender = packetSender;

    public async Task Process(AuthProcessorContext context, ServerCommand packet)
    {
        logger.ZLogInformation($"{context.Sender.Name} issued command '/{packet.Cmd}'");
        string commandOutput = "";
        bool success;
        using (logger.BeginPlainScope())
        using (CaptureScope scope = logger.BeginCaptureScope())
        {
            success = cmdProcessor.ExecuteCommand(packet.Cmd, new PlayerToServerCommandContext(packetSender, context.Sender), out Task<bool>? task);
            if (task != null)
            {
                success = success && await task;
            }

            commandOutput = string.Join("", scope.Logs);
        }

        // Only log back to user if there are errors. Otherwise, successful commands will log themselves.
        if (!success)
        {
            await context.ReplyAsync(new ChatMessage(SessionId.SERVER_ID, commandOutput.Trim('\r', '\n')));
        }
        else if (!string.IsNullOrWhiteSpace(commandOutput))
        {
            logger.ZLogInformation($"{commandOutput}");
        }
    }
}
