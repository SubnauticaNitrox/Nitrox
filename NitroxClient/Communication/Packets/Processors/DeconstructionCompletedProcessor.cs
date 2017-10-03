using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class DeconstructionCompletedProcessor : ClientPacketProcessor<DeconstructionCompleted>
    {
        public override void Process(DeconstructionCompleted packet)
        {
            GameObject deconstructing = GuidHelper.RequireObjectFrom(packet.Guid);
            UnityEngine.Object.Destroy(deconstructing);            
        }
    }
}
