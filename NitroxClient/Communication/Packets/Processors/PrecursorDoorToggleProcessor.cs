using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class PrecursorDoorToggleProcessor : ClientPacketProcessor<NitroxModel.Packets.PrecursorDoorwayToggle>
    {
        private readonly IPacketSender packetSender;

        public PrecursorDoorToggleProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(NitroxModel.Packets.PrecursorDoorwayToggle packet)
        {
            GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.Id);
            PrecursorDoorway precursorDoorway = gameObject.GetComponent<PrecursorDoorway>();

            if (packet.ToggleDoorway != precursorDoorway.isOpen)
            {
                using (packetSender.Suppress<NitroxModel.Packets.PrecursorDoorwayToggle>())
                {
                    precursorDoorway.ToggleDoor(packet.ToggleDoorway);
                }
            }
        }
    }
}
