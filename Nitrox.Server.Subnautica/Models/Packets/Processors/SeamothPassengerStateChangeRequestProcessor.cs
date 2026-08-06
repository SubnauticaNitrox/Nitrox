using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SeamothPassengerStateChangeRequestProcessor(SeamothPassengerService passengerService) : IAuthPacketProcessor<SeamothPassengerStateChangeRequest>
{
    public async Task Process(AuthProcessorContext context, SeamothPassengerStateChangeRequest packet)
    {
        SeamothPassengerChangeResult result = passengerService.Change(context.Sender, packet.SeamothId);
        if (result.Status == SeamothPassengerChangeStatus.Changed)
        {
            await context.SendToAllAsync(result.State);
        }
        else
        {
            await context.ReplyAsync(result.State);
        }
    }
}
