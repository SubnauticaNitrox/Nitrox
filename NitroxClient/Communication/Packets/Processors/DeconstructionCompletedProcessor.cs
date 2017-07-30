using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class DeconstructionCompletedProcessor : ClientPacketProcessor<DeconstructionCompleted>
    {
        public override void Process(DeconstructionCompleted packet)
        {
            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(packet.Guid);

            if(opGameObject.IsPresent())
            {
                GameObject deconstructing = opGameObject.Get();

                UnityEngine.Object.Destroy(deconstructing);
            }
        }
    }
}
