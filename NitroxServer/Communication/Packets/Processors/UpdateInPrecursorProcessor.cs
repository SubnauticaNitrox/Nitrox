using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;

namespace NitroxServer.Communication.Packets.Processors;

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
