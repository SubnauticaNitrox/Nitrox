using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class FabricatorStateChangedProcessor : ClientPacketProcessor<FabricatorStateChanged>
{

    public override void Process(FabricatorStateChanged packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject fabricatorObject))
        {
            Log.Error($"[FabricatorStateChangedProcessor] Couldn't find GameObject for {packet.Id}");
            return;
        }

        if (!fabricatorObject.TryGetComponent<Fabricator>(out Fabricator fabricator))
        {
            Log.Info("[FabricatorStateChangedProcessor] Unable to find Fabricator component");
            return;
        }

        PlayFabricatorSound(fabricator, packet.IsCrafting);
    }

    private void PlayFabricatorSound(Fabricator fabricator, bool isCrafting)
    {
        if (isCrafting)
        {
            fabricator.fabricateSound.Play();
        }
        else
        {
            fabricator.fabricateSound.Stop();
        }
    }
}
