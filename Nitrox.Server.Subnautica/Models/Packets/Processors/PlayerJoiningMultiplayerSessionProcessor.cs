using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerJoiningMultiplayerSessionProcessor(JoiningManager joiningManager) : IAnonPacketProcessor<PlayerJoiningMultiplayerSession>
{
    private readonly JoiningManager joiningManager = joiningManager;

    public Task Process(AnonProcessorContext context, PlayerJoiningMultiplayerSession packet)
    {
        joiningManager.AddToJoinQueue(context.Sender.SessionId, packet.ReservationKey);
        return Task.CompletedTask;
    }
}
