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

        public override bool Equals(object obj)
        {
            if (obj is SerializableRoomFire)
            {
                return Room == ((SerializableRoomFire)obj).Room;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Room.GetHashCode();
        }
    }
}
