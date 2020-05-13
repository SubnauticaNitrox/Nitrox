using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class RocketToggleElevatorProcessor : ClientPacketProcessor<RocketToggleElevator>
    {
        private readonly IPacketSender packetSender;

        public RocketToggleElevatorProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(RocketToggleElevator packet)
        {
            using (packetSender.Suppress<RocketToggleElevator>())
            {
                GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.Id);
                Rocket rocket = gameObject.RequireComponent<Rocket>();
                rocket.CallElevator(packet.Up);
            }
        }
    }
}
