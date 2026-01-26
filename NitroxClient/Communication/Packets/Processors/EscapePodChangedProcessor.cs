using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class EscapePodChangedProcessor(PlayerManager remotePlayerManager) : IClientPacketProcessor<EscapePodChanged>
{
    private readonly PlayerManager remotePlayerManager = remotePlayerManager;

    public Task Process(ClientProcessorContext context, EscapePodChanged packet)
    {
        Optional<RemotePlayer> remotePlayer = remotePlayerManager.Find(packet.SessionId);

        if (remotePlayer.HasValue)
        {
            EscapePod? escapePod = null;

            if (packet.EscapePodId.HasValue)
            {
                GameObject sub = NitroxEntity.RequireObjectFrom(packet.EscapePodId.Value);
                escapePod = sub.GetComponent<EscapePod>();
            }

            remotePlayer.Value.SetEscapePod(escapePod);
        }
        return Task.CompletedTask;
    }
}
