using System;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlaceFurniture : RangedPacket
    {
        public string Guid { get; }
        public Optional<string> SubGuid { get; }
        public Vector3 ItemPosition { get; }
        public Quaternion Rotation { get; }
        public TechType TechType { get; }
        public Vector3 CameraPosition { get; }
        public Quaternion CameraRotation { get; }

        public PlaceFurniture(string guid, Optional<string> subGuid, Vector3 itemPosition, Quaternion rotation, Vector3 cameraPosition, Quaternion cameraRotation, TechType techType) : base(itemPosition, 3)
        {
            Guid = guid;
            SubGuid = subGuid;
            ItemPosition = itemPosition;
            Rotation = rotation;
            CameraPosition = cameraPosition;
            CameraRotation = cameraRotation;
            TechType = techType;
        }

        public override string ToString()
        {
            return "[PlaceFurniture - ItemPosition: " + ItemPosition + " Guid: " + Guid + " SubGuid: " + SubGuid + " Rotation: " + Rotation + " CameraPosition: " + CameraPosition + "CameraRotation: " + CameraRotation + " TechType: " + TechType + "]";
        }
    }
}
