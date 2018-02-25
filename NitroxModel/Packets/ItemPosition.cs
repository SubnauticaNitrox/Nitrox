using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ItemPosition : Packet
    {
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }

        public string Guid { get; }

        public ItemPosition(string guid, Vector3 position, Quaternion rotation)
        {
            Guid = guid;
            Position = position;
            Rotation = rotation;
        }

        public override string ToString()
        {
            return "[ItemPosition position: " + Position + " Rotation: " + Rotation + " guid: " + Guid + "]";
        }
    }
}
