using NitroxModel.DataStructures.Util;
using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlaceBasePiece : PlayerActionPacket
    {
        public String Guid { get; }
        public Vector3 ItemPosition { get; }
        public Quaternion Rotation { get; }
        public TechType TechType { get; }
        public Optional<String> ParentBaseGuid { get; }
        public Transform Camera { get; }

        public PlaceBasePiece(String playerId, String guid, Vector3 itemPosition, Quaternion rotation, Transform camera, TechType techType, Optional<String> parentBaseGuid) : base(playerId, itemPosition)
        {
            this.Guid = guid;
            this.ItemPosition = itemPosition;
            this.Rotation = rotation;
            this.TechType = techType;
            this.Camera = camera;
            this.ParentBaseGuid = parentBaseGuid;
        }
        
        public override string ToString()
        {
            return "[PlaceBasePiece - ItemPosition: " + ItemPosition + " Guid: " + Guid + " Rotation: " + Rotation + " Camera: " + Camera + " TechType: " + TechType + " ParentBaseGuid: " + ParentBaseGuid + "]";
        }
    }
}
