using System;
using System.Linq;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    public class SerializableRoomFire
    {
        public CyclopsRooms Room;
        public SerializableFireNode[] ActiveFireNodes;

        public override string ToString()
        {
            return "[SerializableRoomFire" 
                + " Room: " + Room.ToString()
                + " SerializableFireNodes: " + string.Join(", ", ActiveFireNodes.Select(x => x.FireGuid).ToArray())
                + "]";
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is SerializableRoomFire)
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
