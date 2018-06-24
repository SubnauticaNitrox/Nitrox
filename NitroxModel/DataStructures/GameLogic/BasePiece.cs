﻿using NitroxModel.DataStructures.Util;
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
            get { return (ParentGuid.IsPresent()) ? ParentGuid.Get() : null; }
            set { ParentGuid = Optional<string>.OfNullable(value); }
        }

        [ProtoIgnore]
        public Optional<string> ParentGuid { get; set; }

        [ProtoMember(6)]
        public Vector3 CameraPosition { get; set; }

        [ProtoMember(7)]
        public Quaternion CameraRotation { get; set; }

        [ProtoMember(8)]
        public float ConstructionAmount { get; set; }

        [ProtoMember(9)]
        public bool ConstructionCompleted { get; set; }

        [ProtoMember(10)]
        public bool IsFurniture { get; set; }

        [ProtoMember(11)]
        public string SerializableNewBaseGuid
        {
            get { return (NewBaseGuid.IsPresent()) ? NewBaseGuid.Get() : null; }
            set { NewBaseGuid = Optional<string>.OfNullable(value); }
        }

        [ProtoIgnore]
        public Optional<string> NewBaseGuid { get; set; }

        public BasePiece()
        {
            NewBaseGuid = Optional<String>.Empty();
            ParentGuid = Optional<String>.Empty();
        }

        public BasePiece(string guid, Vector3 itemPosition, Quaternion rotation, Vector3 cameraPosition, Quaternion cameraRotation, TechType techType, Optional<string> parentGuid, bool isFurniture)
        {
            Guid = guid;
            ItemPosition = itemPosition;
            Rotation = rotation;
            TechType = techType;
            CameraPosition = cameraPosition;
            CameraRotation = cameraRotation;
            ParentGuid = parentGuid;
            IsFurniture = isFurniture;
            ConstructionAmount = 0.0f;
            ConstructionCompleted = false;
            NewBaseGuid = Optional<string>.Empty();
        }

        public override string ToString()
        {
            return "[BasePiece - ItemPosition: " + ItemPosition + " Guid: " + Guid + " Rotation: " + Rotation + " CameraPosition: " + CameraPosition + "CameraRotation: " + CameraRotation + " TechType: " + TechType + " ParentGuid: " + ParentGuid + " ConstructionAmount: " + ConstructionAmount + " IsFurniture: " + IsFurniture + " NewBaseGuid: " + NewBaseGuid + "]";
        }
    }
}
