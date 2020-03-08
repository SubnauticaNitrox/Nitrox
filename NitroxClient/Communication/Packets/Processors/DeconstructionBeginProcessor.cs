using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

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
            Log.Info("Received deconstruction packet for basePieceId: " + packet.Id);

            GameObject deconstructing = NitroxEntity.RequireObjectFrom(packet.Id);
            BaseDeconstructable baseDeconstructable = deconstructing.RequireComponent<BaseDeconstructable>();
            
            TransientLocalObjectManager.Add(TransientObjectType.LATEST_DECONSTRUCTED_BASE_PIECE_GUID, packet.Id);

            using (packetSender.Suppress<DeconstructionBegin>())
            {
                baseDeconstructable.Deconstruct();
            }
        }
    }
}
