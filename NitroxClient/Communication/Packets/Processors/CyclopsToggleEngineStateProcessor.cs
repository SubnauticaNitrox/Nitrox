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
    public class CyclopsToggleEngineStateProcessor : ClientPacketProcessor<CyclopsToggleEngineState>
    {
        private readonly IPacketSender packetSender;
        private readonly Cyclops cyclops;

        public CyclopsToggleEngineStateProcessor(IPacketSender packetSender, Cyclops cyclops)
        {
            this.packetSender = packetSender;
            this.cyclops = cyclops;
        }

        public override void Process(CyclopsToggleEngineState enginePacket)
        {
            cyclops.ToggleEngineState(enginePacket.Guid, enginePacket.IsStarting, enginePacket.IsOn);
        }
    }
}
