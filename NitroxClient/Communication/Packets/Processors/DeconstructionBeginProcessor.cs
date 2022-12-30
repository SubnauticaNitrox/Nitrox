﻿using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class DeconstructionBeginProcessor : ClientPacketProcessor<DeconstructionBegin>
    {
        private readonly IPacketSender packetSender;

        public DeconstructionBeginProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(DeconstructionBegin packet)
        {
            Log.Info($"Received deconstruction packet for id: {packet.Id}");

            GameObject deconstructing = NitroxEntity.RequireObjectFrom(packet.Id);

            Constructable constructable = deconstructing.GetComponent<Constructable>();
            BaseDeconstructable baseDeconstructable = deconstructing.GetComponent<BaseDeconstructable>();

            if (baseDeconstructable != null)
            {
                TransientLocalObjectManager.Add(TransientLocalObjectManager.TransientObjectType.LATEST_DECONSTRUCTED_BASE_PIECE_GUID, packet.Id);
                baseDeconstructable.Deconstruct();
            }
            else if (constructable != null)
            {
                constructable.SetState(false, false);
            }
        }
    }
}
