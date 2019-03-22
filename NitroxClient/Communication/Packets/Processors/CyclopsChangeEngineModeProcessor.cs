using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsChangeEngineModeProcessor : ClientPacketProcessor<CyclopsChangeEngineMode>
    {
        private readonly IPacketSender packetSender;

        public CyclopsChangeEngineModeProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsChangeEngineMode motorPacket)
        {
            GameObject cyclops = GuidHelper.RequireObjectFrom(motorPacket.Guid);
            CyclopsMotorMode motorMode = cyclops.RequireComponentInChildren<CyclopsMotorMode>();
            motorMode.BroadcastMessage("SetCyclopsMotorMode", motorPacket.Mode, SendMessageOptions.RequireReceiver);
        }
    }
}
