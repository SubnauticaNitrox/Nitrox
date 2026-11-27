using System.Collections;
using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;
using UWE;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PlayerAnimatorStateUpdateProcessor(PlayerManager remotePlayerManager) : IClientPacketProcessor<PlayerAnimatorStateUpdate>
{
    private readonly PlayerManager remotePlayerManager = remotePlayerManager;

    public Task Process(ClientProcessorContext context, PlayerAnimatorStateUpdate packet)
    {
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

    private void UpdateAnimation(PlayerAnimatorStateUpdate packet)
    {
        Optional<RemotePlayer> remotePlayer = remotePlayerManager.Find(packet.SessionId);
        if (remotePlayer.HasValue)
        {
            foreach (KeyValuePair<string, bool> parameter in packet.AnimatorParameters)
            {
                remotePlayer.Value.PlayerContext.AnimatorParameters[parameter.Key] = parameter.Value;
            }
            remotePlayer.Value.UpdateAnimatorParameters(packet.AnimatorParameters);
        }
    }
}
