using NitroxModel.DataStructures;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public abstract class PlayerActionPacket : PlayerPacket
    {
        public Vector3 ActionPosition { get; protected set; }
        public bool PlayerMustBeInRangeToReceive { get; protected set; }

        public PlayerActionPacket(String playerId, Vector3 eventPosition) : base(playerId)
        {
            this.ActionPosition = eventPosition;
            this.PlayerMustBeInRangeToReceive = true;
        }
    }
}
