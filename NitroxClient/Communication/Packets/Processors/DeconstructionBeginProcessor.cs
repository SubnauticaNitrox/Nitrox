using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
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
            Log.Info($"Received deconstruction packet for id: {packet.Id}");
            // The gameObject that we are deconstructing
            GameObject deconstructing = NitroxEntity.RequireObjectFrom(packet.Id);

            Constructable constructable = deconstructing.GetComponent<Constructable>();
            BaseDeconstructable baseDeconstructable = deconstructing.GetComponent<BaseDeconstructable>();

            using (PacketSuppressor<DeconstructionBegin>.Suppress())
            {
                // If the base piece can be deconstructed, deconstruct it
                if (baseDeconstructable != null)
                {
                    TransientLocalObjectManager.Add(TransientObjectType.LATEST_DECONSTRUCTED_BASE_PIECE_GUID, packet.Id);
                    baseDeconstructable.Deconstruct();
                }
                // If it isn't deconstructable, then call this function that disables the GameObject or the building component of it... i think?
                else if (constructable != null)
                {
                    constructable.SetState(false, false);
                }
            }
        }
    }
}
