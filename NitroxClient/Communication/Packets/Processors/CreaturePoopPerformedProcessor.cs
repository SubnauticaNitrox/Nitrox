using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class CreaturePoopPerformedProcessor : ClientPacketProcessor<CreaturePoopPerformed>
{
    public override void Process(CreaturePoopPerformed packet)
    {
        AI.CreaturePoopPerformed(packet.CreatureId);
    }
}
