using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;
using NitroxModel.DataStructures;
using NitroxModel.Logger;

namespace NitroxClient.GameLogic
{
    public class PrecursorManager
    {
        private readonly IPacketSender packetSender;
        public PrecursorManager(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void TogglePrecursorDoor(NitroxId id, bool toggle)
        {
            PrecursorDoorwayToggle doorway = new PrecursorDoorwayToggle(id, toggle);
            packetSender.Send(doorway);
        }
    }
}
