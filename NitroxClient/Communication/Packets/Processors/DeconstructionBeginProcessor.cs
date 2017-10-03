using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Helper.Unity;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class DeconstructionBeginProcessor : ClientPacketProcessor<DeconstructionBegin>
    {
        public override void Process(DeconstructionBegin packet)
        {
            GameObject deconstructing = GuidHelper.RequireObjectFrom(packet.Guid);
            Constructable constructable = deconstructing.RequireComponent<Constructable>();
            
            constructable.SetState(false, false);
        }
    }
}
