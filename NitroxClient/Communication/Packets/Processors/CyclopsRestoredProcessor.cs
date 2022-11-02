using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class CyclopsRestoredProcessor : ClientPacketProcessor<CyclopsRestored>
{
    // TODO: Complete this restoring part, it doesn't actually work
    public override void Process(CyclopsRestored packet)
    {
        Optional<GameObject> cyclops = NitroxEntity.GetObjectFrom(packet.Id);
        if (!cyclops.HasValue)
        {
            return;
        }
        cyclops.Value.RequireComponent<CyclopsDestructionEvent>().RestoreCyclops();
    }
}
