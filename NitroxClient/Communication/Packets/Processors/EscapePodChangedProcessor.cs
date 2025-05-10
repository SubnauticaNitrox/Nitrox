using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Networking.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class EscapePodChangedProcessor : IClientPacketProcessor<EscapePodChanged>
    {
        private readonly PlayerManager remotePlayerManager;

        public EscapePodChangedProcessor(PlayerManager remotePlayerManager)
        {
            this.remotePlayerManager = remotePlayerManager;
        }

        public Task Process(IPacketProcessContext context, EscapePodChanged packet)
        {
            Optional<RemotePlayer> remotePlayer = remotePlayerManager.Find(packet.PlayerId);

            if (remotePlayer.HasValue)
            {
                EscapePod escapePod = null;

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
}
