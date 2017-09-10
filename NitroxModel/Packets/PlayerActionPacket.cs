using NitroxModel.DataStructures;
using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public abstract class PlayerActionPacket : AuthenticatedPacket
    {
        public Vector3 ActionPosition { get { return SerializedActionPosition.ToVector3(); } }
        public bool PlayerMustBeInRangeToReceive { get; protected set; }

        private SerializableVector3 SerializedActionPosition { get; }

        public PlayerActionPacket(String playerId, Vector3 eventPosition) : base(playerId)
        {
            this.SerializedActionPosition = SerializableVector3.from(eventPosition);
            this.PlayerMustBeInRangeToReceive = true;
        }

    }
}
