using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class BaseDeconstructionCompletedProcessor : ClientPacketProcessor<BaseDeconstructionCompleted>
    {
        public override void Process(BaseDeconstructionCompleted packet)
        {
            GameObject deconstructing = NitroxEntity.RequireObjectFrom(packet.Id);
            UnityEngine.Object.Destroy(deconstructing);
        }
    }
}
