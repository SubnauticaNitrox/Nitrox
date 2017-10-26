using System;
using NitroxModel.DataStructures.Util;
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
        public Vector3 CameraPosition { get; }
        public Quaternion CameraRotation { get; }

        public PlaceBasePiece(String playerId, String guid, Vector3 itemPosition, Quaternion rotation, Vector3 cameraPosition, Quaternion cameraRotation, TechType techType, Optional<String> parentBaseGuid) : base(playerId, itemPosition)
        {
            Guid = guid;
            ItemPosition = itemPosition;
            Rotation = rotation;
            TechType = techType;
            CameraPosition = cameraPosition;
            CameraRotation = cameraRotation;
            ParentBaseGuid = parentBaseGuid;
        }

        public override string ToString()
        {
            return "[PlaceBasePiece - ItemPosition: " + ItemPosition + " Guid: " + Guid + " Rotation: " + Rotation + " CameraPosition: " + CameraPosition + "CameraRotation: " + CameraRotation + " TechType: " + TechType + " ParentBaseGuid: " + ParentBaseGuid + "]";
        }
    }
}
