using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class LeakRepairedProcessor : ClientPacketProcessor<LeakRepaired>
{
    public override void Process(LeakRepaired packet)
    {
        if (NitroxEntity.TryGetComponentFrom(packet.BaseId, out BaseLeakManager baseLeakManager))
        {
            baseLeakManager.HealLeakToMax(packet.RelativeCell.ToUnity());
        }
    }
}
