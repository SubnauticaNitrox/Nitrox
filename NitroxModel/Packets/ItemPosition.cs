using System;
using System.Collections.Generic;
using System.Text;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ItemPosition : PlayerActionPacket
    {
        public String Guid { get; protected set; }
        public Vector3 Position { get; protected set; }
        public Quaternion Rotation { get; protected set; }

        public ItemPosition(String playerId, String guid, Vector3 position, Quaternion rotation) : base(playerId, position)
        {
            this.Guid = guid;
            this.Position = position;
            this.Rotation = rotation;
            this.PlayerMustBeInRangeToReceive = false;
        }

        public override string ToString()
        {
            return "[ItemPosition - playerId: " + PlayerId + " position: " + Position + " Rotation: " + Rotation + " guid: " + Guid + "]";
        }
    }
}
