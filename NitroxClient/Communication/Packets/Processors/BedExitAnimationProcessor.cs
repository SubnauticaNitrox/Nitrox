using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.BedSync;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class BedExitAnimationProcessor(PlayerManager playerManager) : IClientPacketProcessor<BedExitAnimation>
{
    private readonly PlayerManager playerManager = playerManager;

    public async Task Process(ClientProcessorContext context, BedExitAnimation packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.BedId, out GameObject bedObject) ||
            !bedObject.TryGetComponent(out RemoteBedController bedController) ||
            !playerManager.TryFind(packet.SessionId, out RemotePlayer remotePlayer))
        {
            return;
        }

        // End the current lie-down/idle animation first
        bedController.EndBedAnimation(remotePlayer, packet.AnimationKey);
        
        // Wait one frame to ensure cleanup is complete
        await Task.Yield();
        
        // Then start the stand-up animation
        bedController.StartBedAnimation(remotePlayer, packet.AnimationKey);
    }
}
