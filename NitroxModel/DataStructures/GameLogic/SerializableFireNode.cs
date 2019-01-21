using System;

namespace NitroxModel.DataStructures.GameLogic
{
    /// <summary>
    /// Used in conjunction with <see cref="SerializableRoomFire"/> to pass fire information across the network.
    /// </summary>
    [Serializable]
    public struct SerializableFireNode
    {
        public string FireGuid;
        public int NodeIndex;

        public override string ToString()
        {
            return "[SerializableFireNode"
                + " FireGuid: " + FireGuid
                + " NodeIndex: " + NodeIndex
                + "]";
        }
    }
}
