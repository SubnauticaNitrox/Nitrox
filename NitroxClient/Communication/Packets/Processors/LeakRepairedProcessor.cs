using NitroxClient.MonoBehaviours;
using Nitrox.Model.Subnautica.DataStructures;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class LeakRepairedProcessor : IClientPacketProcessor<LeakRepaired>
{
    public Task Process(IPacketProcessContext context, LeakRepaired packet)
    {
        if (NitroxEntity.TryGetComponentFrom(packet.BaseId, out BaseLeakManager baseLeakManager))
        {
            baseLeakManager.HealLeakToMax(packet.RelativeCell.ToUnity());
        }

        return Task.CompletedTask;
    }
}
