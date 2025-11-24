using System.Collections;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;
using UWE;

namespace NitroxClient.Communication.Packets.Processors;

public class AnimationChangeEventProcessor : ClientPacketProcessor<AnimationChangeEvent>
{
    private readonly PlayerManager remotePlayerManager;

    public AnimationChangeEventProcessor(PlayerManager remotePlayerManager)
    {
        this.remotePlayerManager = remotePlayerManager;
    }

    public override void Process(AnimationChangeEvent animEvent)
    {
        // Possible for this to be sent during initial sync when the RemotePlayer doesn't exist yet
        if (Multiplayer.Main.InitialSyncCompleted)
        {
            UpdateAnimation(animEvent);
        }
        else
        {
            CoroutineHost.StartCoroutine(Coroutine());
            IEnumerator Coroutine()
            {
                yield return new WaitUntil(() => Multiplayer.Main.InitialSyncCompleted);
                UpdateAnimation(animEvent);
            }
        }
    }

    private void UpdateAnimation(AnimationChangeEvent animEvent)
    {
        Optional<RemotePlayer> opPlayer = remotePlayerManager.Find(animEvent.PlayerId);
        if (opPlayer.HasValue)
        {
            PlayerAnimation playerAnimation = animEvent.Animation;
            opPlayer.Value.UpdateAnimationAndCollider(playerAnimation.Type, playerAnimation.State);
        }
    }
}
