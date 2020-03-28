using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class SubRootChangedProcessor : ClientPacketProcessor<SubRootChanged>
    {
        private readonly PlayerManager remotePlayerManager;

        public SubRootChangedProcessor(PlayerManager remotePlayerManager)
        {
            this.remotePlayerManager = remotePlayerManager;
        }

        public override void Process(SubRootChanged packet)
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
        }
    }
}
