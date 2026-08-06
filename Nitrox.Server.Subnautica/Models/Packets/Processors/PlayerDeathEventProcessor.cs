using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerDeathEventProcessor(IOptions<SubnauticaServerOptions> config, SeamothPassengerService passengerService) : IAuthPacketProcessor<PlayerDeathEvent>
{
    private readonly IOptions<SubnauticaServerOptions> options = config;

    public async Task Process(AuthProcessorContext context, PlayerDeathEvent packet)
    {
        foreach (SeamothPassengerStateChanged state in passengerService.ClearPlayerLifecycle(context.Sender))
        {
            await context.SendToAllAsync(state);
        }

        if (options.Value.IsHardcore())
        {
            context.Sender.IsPermaDeath = true;
            await context.ReplyAsync(new PlayerKicked("Permanent death from hardcore mode"));
        }
        context.Sender.LastStoredPosition = packet.DeathPosition;
        context.Sender.LastStoredSubRootID = context.Sender.SubRootId;
        if (context.Sender.Permissions > Perms.MODERATOR)
        {
            await context.ReplyAsync(new ChatMessage(SessionId.SERVER_ID, "You can use /back to go to your death location"));
        }
        await context.SendToOthersAsync(packet);
    }
}
