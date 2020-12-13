using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using UnityEngine;

namespace Nitrox.Client.Communication.Packets.Processors
{
    public class EscapePodChangedProcessor : ClientPacketProcessor<EscapePodChanged>
    {
        private readonly PlayerManager remotePlayerManager;

        public EscapePodChangedProcessor(PlayerManager remotePlayerManager)
        {
            this.remotePlayerManager = remotePlayerManager;
        }

        public override void Process(EscapePodChanged packet)
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
        }
    }
}


