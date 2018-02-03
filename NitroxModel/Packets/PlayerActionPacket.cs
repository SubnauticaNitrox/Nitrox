using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public abstract class PlayerActionPacket : Packet
    {
        public Vector3 ActionPosition { get; }
        public bool PlayerMustBeInRangeToReceive { get; protected set; }

        public PlayerActionPacket(Vector3 eventPosition)
        {
            ActionPosition = eventPosition;
            PlayerMustBeInRangeToReceive = true;
        }
    }
}
