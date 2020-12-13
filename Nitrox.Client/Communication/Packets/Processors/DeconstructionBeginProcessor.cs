using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic.Helper;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.Logger;
using Nitrox.Model.Packets;
using UnityEngine;
using static Nitrox.Client.GameLogic.Helper.TransientLocalObjectManager;

namespace Nitrox.Client.Communication.Packets.Processors
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
            Log.Info("Received deconstruction packet for id: " + packet.Id);

            GameObject deconstructing = NitroxEntity.RequireObjectFrom(packet.Id);
            
            Constructable constructable = deconstructing.GetComponent<Constructable>();
            BaseDeconstructable baseDeconstructable = deconstructing.GetComponent<BaseDeconstructable>();

            using (packetSender.Suppress<DeconstructionBegin>())
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
        }
    }
}
