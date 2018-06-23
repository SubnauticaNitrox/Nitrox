using NitroxModel.DataStructures.Util;
using ProtoBuf;
using System;
using UnityEngine;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class BasePiece
    {
        [ProtoMember(1)]
        public string Guid { get; set; }

        [ProtoMember(2)]
        public Vector3 ItemPosition { get; set; }

        [ProtoMember(3)]
        public Quaternion Rotation { get; set; }

        [ProtoMember(4)]
        public TechType TechType { get; set; }

        [ProtoMember(5)]
        public string SerializableParentBaseGuid {
            get { return (ParentBaseGuid.IsPresent()) ? ParentBaseGuid.Get() : null; }
            set { ParentBaseGuid = Optional<string>.OfNullable(value); }
        }

        [ProtoIgnore]
        public Optional<string> ParentBaseGuid { get; set; }

        [ProtoMember(6)]
        public Vector3 CameraPosition { get; set; }

        [ProtoMember(7)]
        public Quaternion CameraRotation { get; set; }

        [ProtoMember(8)]
        public float ConstructionAmount { get; set; }

        [ProtoMember(9)]
        public bool ConstructionCompleted { get; set; }

        public BasePiece()
        {
            // Constructor for serialization
        }

        public BasePiece(string guid, Vector3 itemPosition, Quaternion rotation, Vector3 cameraPosition, Quaternion cameraRotation, TechType techType, Optional<string> parentBaseGuid)
        {
            Guid = guid;
            ItemPosition = itemPosition;
            Rotation = rotation;
            TechType = techType;
            CameraPosition = cameraPosition;
            CameraRotation = cameraRotation;
            ParentBaseGuid = parentBaseGuid;
            ConstructionAmount = 0.0f;
            ConstructionCompleted = false;
        }

        public override string ToString()
        {
            return "[BasePiece - ItemPosition: " + ItemPosition + " Guid: " + Guid + " Rotation: " + Rotation + " CameraPosition: " + CameraPosition + "CameraRotation: " + CameraRotation + " TechType: " + TechType + " ParentBaseGuid: " + ParentBaseGuid + " ConstructionAmount: " + ConstructionAmount + "]";
        }
    }
}
