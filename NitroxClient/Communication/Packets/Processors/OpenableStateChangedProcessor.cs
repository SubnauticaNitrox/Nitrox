using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class OpenableStateChangedProcessor : ClientPacketProcessor<OpenableStateChanged>
    {
        private readonly IPacketSender packetSender;

        public OpenableStateChangedProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(OpenableStateChanged packet)
        {
            GameObject gameObject = GuidHelper.RequireObjectFrom(packet.Guid);            
            Openable openable = gameObject.RequireComponent<Openable>();
            
            using (packetSender.Suppress<OpenableStateChanged>())
            {
                openable.PlayOpenAnimation(packet.IsOpen, packet.Duration);
            }
        }
    }
}
