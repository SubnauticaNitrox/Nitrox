using System;
using NitroxModel.DataStructures;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ItemPosition : PlayerActionPacket
    {
        public Vector3 Position { get { return serializablePosition.ToVector3(); } }
        public Quaternion Rotation { get { return serializableRotation.ToQuaternion(); } }

        public String Guid { get; }
        public SerializableVector3 serializablePosition { get; }
        public SerializableQuaternion serializableRotation { get; }

        public ItemPosition(String playerId, String guid, Vector3 position, Quaternion rotation) : base(playerId, position)
        {
            this.Guid = guid;
            this.serializablePosition = SerializableVector3.from(position);
            this.serializableRotation = SerializableQuaternion.from(rotation);
            this.PlayerMustBeInRangeToReceive = false;
        }

        public override string ToString()
        {
            return "[ItemPosition - playerId: " + PlayerId + " position: " + Position + " Rotation: " + Rotation + " guid: " + Guid + "]";
        }
    }
}
