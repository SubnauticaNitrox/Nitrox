using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class SubRootChangedProcessor(PlayerManager remotePlayerManager) : IClientPacketProcessor<SubRootChanged>
{
    private readonly PlayerManager remotePlayerManager = remotePlayerManager;

    public Task Process(ClientProcessorContext context, SubRootChanged packet)
    {
        Optional<RemotePlayer> remotePlayer = remotePlayerManager.Find(packet.PlayerId);

        if (remotePlayer.HasValue)
        {
            SubRoot subRoot = null;

            if (packet.SubRootId.HasValue)
            {
                GameObject sub = NitroxEntity.RequireObjectFrom(packet.SubRootId.Value);
                subRoot = sub.GetComponent<SubRoot>();
            }

            remotePlayer.Value.SetSubRoot(subRoot);
        }
        return Task.CompletedTask;
    }
}
