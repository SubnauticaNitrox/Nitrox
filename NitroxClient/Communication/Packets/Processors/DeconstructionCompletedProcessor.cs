using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class DeconstructionCompletedProcessor : ClientPacketProcessor<DeconstructionCompleted>
    {
        public override void Process(DeconstructionCompleted packet)
        {
            GameObject deconstructing = NitroxEntity.RequireObjectFrom(packet.Id);
            UnityEngine.Object.Destroy(deconstructing);
        }
    }
}
