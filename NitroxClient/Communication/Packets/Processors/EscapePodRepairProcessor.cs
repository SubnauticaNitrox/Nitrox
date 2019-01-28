using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors
{
    public class EscapePodRepairProcessor : ClientPacketProcessor<EscapePodRepair>
    {
        private readonly EscapePodManager escapePodManager;

        public EscapePodRepairProcessor(EscapePodManager escapePodManager)
        {
            this.escapePodManager = escapePodManager;
        }

        public override void Process(EscapePodRepair packet)
        {
            escapePodManager.OnRepair(packet.Guid);
        }
    }
}
