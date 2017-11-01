using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public abstract class PlayerActionPacket : Packet
    {
        public string PlayerId { get; }
        public Vector3 ActionPosition { get; }
        public bool PlayerMustBeInRangeToReceive { get; protected set; }

        public PlayerActionPacket(string playerId, Vector3 eventPosition)
        {
            PlayerId = playerId;
            ActionPosition = eventPosition;
            PlayerMustBeInRangeToReceive = true;
        }
    }
}
