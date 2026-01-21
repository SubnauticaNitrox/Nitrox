using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

/// <summary>
/// Stores the state of a player being in precursor
/// </summary>
internal sealed class UpdateInPrecursorProcessor : IAuthPacketProcessor<UpdateInPrecursor>
{
    public Task Process(AuthProcessorContext context, UpdateInPrecursor packet)
    {
        context.Sender.InPrecursor = packet.InPrecursor;
        return Task.CompletedTask;
    }
}
