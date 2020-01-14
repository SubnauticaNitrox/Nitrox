﻿using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;
using System;
using UnityEngine;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class BasePiece
    {
        [ProtoMember(1)]
        public NitroxId Id { get; set; }

        [ProtoMember(2)]
        public Vector3 ItemPosition { get; set; }

        [ProtoMember(3)]
        public Quaternion Rotation { get; set; }

        [ProtoMember(4)]
        public TechType TechType { get; set; }

        [ProtoMember(5)]
        public NitroxId SerializableParentBaseId {
            get { return (ParentId.IsPresent()) ? ParentId.Get() : null; }
            set { ParentId = Optional<NitroxId>.OfNullable(value); }
        }

        [ProtoIgnore]
        public Optional<NitroxId> ParentId { get; set; }

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
        public NitroxId BaseId { get; set; }

        [ProtoMember(12, DynamicType = true)]
        public RotationMetadata SerializableRotationMetadata
        {
            get { return (RotationMetadata.IsPresent()) ? RotationMetadata.Get() : null; }
            set { RotationMetadata = Optional<RotationMetadata>.OfNullable(value); }
        }

        [ProtoIgnore]
        public Optional<RotationMetadata> RotationMetadata {get; set; }
        
        [ProtoMember(13, DynamicType = true)]
        public BasePieceMetadata SerializableMetadata
        {
            get { return (Metadata.IsPresent()) ? Metadata.Get() : null; }
            set { Metadata = Optional<BasePieceMetadata>.OfNullable(value); }
        }

        [ProtoIgnore]
        public Optional<BasePieceMetadata> Metadata { get; set; }
                
        public BasePiece()
        {
            ParentId = Optional<NitroxId>.Empty();
            RotationMetadata = Optional<RotationMetadata>.Empty();
            Metadata = Optional<BasePieceMetadata>.Empty();
        }

        public BasePiece(NitroxId id, Vector3 itemPosition, Quaternion rotation, Vector3 cameraPosition, Quaternion cameraRotation, TechType techType, Optional<NitroxId> parentId, bool isFurniture, Optional<RotationMetadata> rotationMetadata)
        {
            Id = id;
            ItemPosition = itemPosition;
            Rotation = rotation;
            TechType = techType;
            CameraPosition = cameraPosition;
            CameraRotation = cameraRotation;
            ParentId = parentId;
            IsFurniture = isFurniture;
            ConstructionAmount = 0.0f;
            ConstructionCompleted = false;
            RotationMetadata = rotationMetadata;
            Metadata = Optional<BasePieceMetadata>.Empty();
        }

        public override string ToString()
        {
            return "[BasePiece - ItemPosition: " + ItemPosition + " Id: " + Id + " Rotation: " + Rotation + " CameraPosition: " + CameraPosition + "CameraRotation: " + CameraRotation + " TechType: " + TechType + " ParentId: " + ParentId + " ConstructionAmount: " + ConstructionAmount + " IsFurniture: " + IsFurniture + " BaseId: " + BaseId + " RotationMetadata: " + RotationMetadata + "]";
        }
    }
}
