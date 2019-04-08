using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class CyclopsDecoyLaunchProcessor : ClientPacketProcessor<CyclopsDecoyLaunch>
    {
        private readonly IPacketSender packetSender;
        private readonly Cyclops cyclops;

        public CyclopsDecoyLaunchProcessor(IPacketSender packetSender, Cyclops cyclops)
        {
            this.packetSender = packetSender;
            this.cyclops = cyclops;
        }

        public override void Process(CyclopsDecoyLaunch decoyLaunchPacket)
        {
            cyclops.LaunchDecoy(decoyLaunchPacket.Id);
        }
    }
}
