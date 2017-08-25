﻿using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlaceBasePiece : PlayerActionPacket
    {
        public String Guid { get; private set; }
        public Vector3 ItemPosition { get; private set; }
        public Quaternion Rotation { get; private set; }
        public Transform Camera { get; private set; }
        public String TechType { get; private set; }
        public Optional<String> ParentBaseGuid { get; private set; }

        public PlaceBasePiece(String playerId, String guid, Vector3 itemPosition, Quaternion rotation, Transform camera, String techType, Optional<String> parentBaseGuid) : base(playerId, itemPosition)
        {
            this.Guid = guid;
            this.ItemPosition = itemPosition;
            this.Rotation = rotation;
            this.TechType = techType;
            this.Camera = camera;
            this.ParentBaseGuid = parentBaseGuid;
            this.PlayerMustBeInRangeToReceive = false;
        }

        public override string ToString()
        {
            return "[PlaceBasePiece - ItemPosition: " + ItemPosition + " Guid: " + Guid + " Rotation: " + Rotation + " Camera: " + Camera + " TechType: " + TechType + " ParentBaseGuid: " + ParentBaseGuid + "]";
        }
    }
}
