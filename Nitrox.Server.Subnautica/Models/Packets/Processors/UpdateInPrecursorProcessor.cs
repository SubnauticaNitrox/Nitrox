using Nitrox.Model.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

/// <summary>
/// Stores the state of a player being in precursor
/// </summary>
public class UpdateInPrecursorProcessor : AuthenticatedPacketProcessor<UpdateInPrecursor>
{
    public override void Process(UpdateInPrecursor packet, Player player)
    {
        player.InPrecursor = packet.InPrecursor;
    }
}
