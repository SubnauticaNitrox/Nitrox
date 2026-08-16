using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.BedSync;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class BedEnterAnimationProcessor(PlayerManager playerManager) : IClientPacketProcessor<BedEnterAnimation>
{
    private readonly PlayerManager playerManager = playerManager;

    public Task Process(ClientProcessorContext context, BedEnterAnimation packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.BedId, out GameObject bedObject) ||
            !bedObject.TryGetComponent(out RemoteBedController bedController) ||
            !playerManager.TryFind(packet.SessionId, out RemotePlayer remotePlayer) ||
            !remotePlayer.Body || !remotePlayer.Body.activeInHierarchy)
        {
            return Task.CompletedTask;
        }

        bedController.StartBedAnimation(remotePlayer, packet.AnimationKey);
        return Task.CompletedTask;
    }
}
