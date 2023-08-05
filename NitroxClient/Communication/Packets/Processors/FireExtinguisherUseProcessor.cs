using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class FireExtinguisherUseProcessor : ClientPacketProcessor<FireExtinguisherUse>
{
    private readonly FireExtinguisherManager extinguisherManager;

    public FireExtinguisherUseProcessor(FireExtinguisherManager extinguisherManager)
    {
        this.extinguisherManager = extinguisherManager;
    }

    public override void Process(FireExtinguisherUse packet)
    {
        if (NitroxEntity.TryGetComponentFrom(packet.ItemId, out FireExtinguisher fireExtinguisher))
        {
            if (packet.Activated)
            {
                extinguisherManager.StartUsing(packet.ItemId, fireExtinguisher);
            }
            else
            {
                extinguisherManager.StopUsing(packet.ItemId);
            }
        }
    }
}
