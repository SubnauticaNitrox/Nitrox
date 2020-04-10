using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;
using NitroxModel.DataStructures;
using NitroxModel.Logger;

namespace NitroxClient.GameLogic
{
    public class PrecursorManager
    {
        private readonly IPacketSender packetSender;
        private readonly IMultiplayerSession multiplayerSession;
        public PrecursorManager(IPacketSender packetSender, IMultiplayerSession multiplayerSession)
        {
            this.packetSender = packetSender;
            this.multiplayerSession = multiplayerSession;
        }

        public void TogglePrecursorDoor(NitroxId id, bool toggle)
        {
            PrecursorDoorwayToggle doorway = new PrecursorDoorwayToggle(id, toggle);
            packetSender.Send(doorway);
        }
    }
}
