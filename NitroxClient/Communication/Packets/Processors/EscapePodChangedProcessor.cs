﻿using Nitrox.Model.DataStructures;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
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
