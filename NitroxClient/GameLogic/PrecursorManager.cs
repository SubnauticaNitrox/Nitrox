using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;
using NitroxModel.DataStructures;

namespace NitroxClient.GameLogic
{
    public class PrecursorManager
    {
        private readonly IPacketSender packetSender;
        public PrecursorManager(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void TogglePrecursorDoor(NitroxId id, bool isOpen)
        {
            PrecursorDoorwayAction doorway = new PrecursorDoorwayAction(id, isOpen);
            packetSender.Send(doorway);
        }
    }
}
