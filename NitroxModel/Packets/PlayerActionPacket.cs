using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public abstract class PlayerActionPacket : AuthenticatedPacket
    {
        public Vector3 ActionPosition { get; }
        public bool PlayerMustBeInRangeToReceive { get; protected set; }

        public PlayerActionPacket(String playerId, Vector3 eventPosition) : base(playerId)
        {
            ActionPosition = eventPosition;
            PlayerMustBeInRangeToReceive = true;
        }
    }
}
