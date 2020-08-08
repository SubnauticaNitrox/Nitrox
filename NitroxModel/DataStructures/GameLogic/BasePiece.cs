using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;
using System;
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
        public NitroxVector3 ItemPosition { get; set; }

        [ProtoMember(3)]
        public NitroxQuaternion Rotation { get; set; }

        [ProtoMember(4)]
        public NitroxTechType TechType { get; set; }

        [ProtoMember(5)]
        public Optional<NitroxId> ParentId { get; set; }

        [ProtoMember(6)]
        public NitroxVector3 CameraPosition { get; set; }

        [ProtoMember(7)]
        public NitroxQuaternion CameraRotation { get; set; }

        [ProtoMember(8)]
        public float ConstructionAmount { get; set; }

        [ProtoMember(9)]
        public bool ConstructionCompleted { get; set; }

        [ProtoMember(10)]
        public bool IsFurniture { get; set; }

        [ProtoMember(11)]
        public NitroxId BaseId { get; set; }

        [ProtoMember(12, DynamicType = true)]
        public Optional<RotationMetadata> RotationMetadata { get; set; }

        [ProtoMember(13, DynamicType = true)]
        public Optional<BasePieceMetadata> Metadata { get; set; }

        protected BasePiece()
        {
            ParentId = Optional.Empty;
            RotationMetadata = Optional.Empty;
            Metadata = Optional.Empty;
        }

        public BasePiece(NitroxId id, NitroxVector3 itemPosition, NitroxQuaternion rotation, NitroxVector3 cameraPosition, NitroxQuaternion cameraRotation, NitroxTechType techType, Optional<NitroxId> parentId, bool isFurniture, Optional<RotationMetadata> rotationMetadata)
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
            Metadata = Optional.Empty;
        }

        public override string ToString()
        {
            return $"[BasePiece - Id: {Id} CameraPosition: {CameraPosition}CameraRotation: {CameraRotation} TechType: {TechType} ConstructionAmount: {ConstructionAmount} IsFurniture: {IsFurniture} BaseId: {BaseId} RotationMetadata: {RotationMetadata}]";
        }
    }
}
