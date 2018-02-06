using System;

namespace NitroxModel.DataStructures.GameLogic
{
    /// <summary>
    /// Used in conjunction with <see cref="SerializableRoomFire"/> to pass fire information across the network.
    /// </summary>
    [Serializable]
    public struct SerializableFireNode
    {
        public int NodeIndex;
        public int FireCount;

        public override string ToString()
        {
            return "[SerializableFireNode"
                + " NodeIndex: " + NodeIndex.ToString()
                + " FireCount: " + FireCount.ToString()
                + "]";
        }
    }
}
