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
        public String SubGuid { get; private set; }
        public Vector3 ItemPosition { get; private set; }
        public Quaternion Rotation { get; private set; }
        public Transform Camera { get; private set; }
        public String TechType { get; private set; }

        public PlaceFurniture(String playerId, String guid, String subGuid, Vector3 itemPosition, Quaternion rotation, Transform camera, String techType) : base(playerId, itemPosition)
        {
            this.Guid = guid;
            this.SubGuid = subGuid;
            this.ItemPosition = itemPosition;
            this.Rotation = rotation;
            this.Camera = camera;
            this.TechType = techType;
        }

        public override string ToString()
        {
            return "[PlaceFurniture - ItemPosition: " + ItemPosition + " Guid: " + Guid + " SubGuid: " + SubGuid + " Rotation: " + Rotation + " Camera: " + Camera + " TechType: " + TechType + "]";
        }
    }
}
