using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ItemPosition : PlayerActionPacket
    {
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }

        public string Guid { get; }

        public ItemPosition(string playerId, string guid, Vector3 position, Quaternion rotation) : base(playerId, position)
        {
            Guid = guid;
            Position = position;
            Rotation = rotation;
            PlayerMustBeInRangeToReceive = false;
        }

        public override string ToString()
        {
            return "[ItemPosition - playerId: " + PlayerId + " position: " + Position + " Rotation: " + Rotation + " guid: " + Guid + "]";
        }
    }
}
