using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

/// <summary>
/// Stores the state of a player being in precursor
/// </summary>
internal sealed class UpdateInPrecursorProcessor : AuthenticatedPacketProcessor<UpdateInPrecursor>
{
    public override void Process(UpdateInPrecursor packet, Player player)
    {
        player.InPrecursor = packet.InPrecursor;
    }
}
