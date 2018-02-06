using System;
using System.Linq;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    public class SerializableRoomFire
    {
        public CyclopsRooms Room;
        public SerializableFireNode[] ActiveRoomFireNodes;

        public override string ToString()
        {
            return "[SerializableRoomFire" 
                + " Room: " + Room.ToString()
                + " SerializableFireNodes: " + string.Join(", ", ActiveRoomFireNodes.Select(x => x.ToString()).ToArray())
                + "]";
        }
    }
}
