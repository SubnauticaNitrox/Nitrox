using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class FabricatorStateChangedProcessor : ClientPacketProcessor<FabricatorStateChanged>
{
    private readonly PlayerManager remotePlayerManager;

    public FabricatorStateChangedProcessor(PlayerManager remotePlayerManager)
    {
        this.remotePlayerManager = remotePlayerManager;
    }

    public override void Process(FabricatorStateChanged packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject fabricatorObject))
        {
            Log.Error($"[FabricatorStateChangedProcessor] Couldn't find GameObject for {packet.Id}");
            return;
        }

        if (!remotePlayerManager.TryFind(packet.PlayerId, out RemotePlayer otherPlayer))
        {
            Log.Error($"[FabricatorStateChangedProcessor] Couldn't find RemotePlayer for {packet.PlayerId}");
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
