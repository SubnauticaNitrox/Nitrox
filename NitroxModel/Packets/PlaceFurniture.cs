using NitroxModel.DataStructures.Util;
using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlaceFurniture : PlayerActionPacket
    {
        public String Guid { get; }
        public Optional<String> SubGuid { get; }
        public Vector3 ItemPosition { get; }
        public Quaternion Rotation { get; }
        public TechType TechType { get; }
        public Transform Camera { get; }

        public PlaceFurniture(String playerId, String guid, Optional<String> subGuid, Vector3 itemPosition, Quaternion rotation, Transform camera, TechType techType) : base(playerId, itemPosition)
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
