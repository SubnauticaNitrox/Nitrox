using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class PrecursorDoorToggleProcessor : ClientPacketProcessor<NitroxModel.Packets.PrecursorDoorwayAction>
    {
        private readonly IPacketSender packetSender;

        public PrecursorDoorToggleProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(NitroxModel.Packets.PrecursorDoorwayAction packet)
        {
            GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.Id);
            PrecursorDoorway precursorDoorway = gameObject.GetComponent<PrecursorDoorway>();

            if (packet.isOpen != precursorDoorway.isOpen)
            {
                using (packetSender.Suppress<NitroxModel.Packets.PrecursorDoorwayAction>())
                {
                    precursorDoorway.ToggleDoor(packet.isOpen);
                }
            }
        }
    }
}
