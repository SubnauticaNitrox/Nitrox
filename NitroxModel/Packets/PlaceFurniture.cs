using NitroxModel.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlaceFurniture : PlayerActionPacket
    {
        public String Guid { get; private set; }
        public Vector3 ItemPosition { get; private set; }
        public Quaternion Rotation { get; private set; }
        public String TechType { get; private set; }

        public PlaceFurniture(String playerId, String guid, Vector3 itemPosition, Quaternion rotation, String techType) : base(playerId, itemPosition)
        {
            this.Guid = guid;
            this.ItemPosition = itemPosition;
            this.Rotation = rotation;
            this.TechType = techType;
        }

        public override string ToString()
        {
            return "[PlaceFurniture - ItemPosition: " + ItemPosition + " Guid" + Guid + " Rotation: " + Rotation + " TechType: " + TechType + "]";
        }
    }
}
