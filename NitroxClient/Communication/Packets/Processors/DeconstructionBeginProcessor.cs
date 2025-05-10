using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.Networking.Packets;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxClient.Communication.Packets.Processors
{
    public class DeconstructionBeginProcessor : IClientPacketProcessor<DeconstructionBegin>
    {
        private readonly IPacketSender packetSender;

        public DeconstructionBeginProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public Task Process(IPacketProcessContext context, DeconstructionBegin packet)
        {
            Log.Info($"Received deconstruction packet for id: {packet.Id}");

            GameObject deconstructing = NitroxEntity.RequireObjectFrom(packet.Id);

            Constructable constructable = deconstructing.GetComponent<Constructable>();
            BaseDeconstructable baseDeconstructable = deconstructing.GetComponent<BaseDeconstructable>();

            using (PacketSuppressor<DeconstructionBegin>.Suppress())
            {
                if (baseDeconstructable != null)
                {
                    TransientLocalObjectManager.Add(TransientObjectType.LATEST_DECONSTRUCTED_BASE_PIECE_GUID, packet.Id);
                    baseDeconstructable.Deconstruct();
                }
                else if (constructable != null)
                {
                    constructable.SetState(false, false);
                }
            }

            return Task.CompletedTask;
        }
    }
}
