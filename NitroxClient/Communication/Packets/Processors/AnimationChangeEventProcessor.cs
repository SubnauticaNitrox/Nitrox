using System.Collections;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;
using UWE;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class AnimationChangeEventProcessor(PlayerManager remotePlayerManager) : IClientPacketProcessor<AnimationChangeEvent>
{
    private readonly PlayerManager remotePlayerManager = remotePlayerManager;

    public Task Process(ClientProcessorContext context, AnimationChangeEvent packet)
    {
        // Possible for this to be sent during initial sync when the RemotePlayer doesn't exist yet
        if (Multiplayer.Main.InitialSyncCompleted)
        {
            UpdateAnimation(packet);
        }
        else
        {
            CoroutineHost.StartCoroutine(Coroutine());

            IEnumerator Coroutine()
            {
                yield return new WaitUntil(() => Multiplayer.Main.InitialSyncCompleted);
                UpdateAnimation(packet);
            }
        }
        return Task.CompletedTask;
    }

    private void UpdateAnimation(AnimationChangeEvent animEvent)
    {
        Optional<RemotePlayer> opPlayer = remotePlayerManager.Find(animEvent.SessionId);
        if (opPlayer.HasValue)
        {
            PlayerAnimation playerAnimation = animEvent.Animation;
            opPlayer.Value.UpdateAnimationAndCollider(playerAnimation.Type, playerAnimation.State);
        }
    }
}
