using System;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;

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

        [ProtoMember(5, DynamicType = true)]
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
        public Optional<BuilderMetadata> RotationMetadata { get; set; }

        [ProtoMember(13, DynamicType = true)]
        public Optional<BasePieceMetadata> Metadata { get; set; }

        [ProtoMember(14, DynamicType = true)]
        public int BuildIndex { get; set; }

        protected BasePiece()
        {
            ParentId = Optional.Empty;
            RotationMetadata = Optional.Empty;
            Metadata = Optional.Empty;
        }

        public BasePiece(NitroxId id, NitroxVector3 itemPosition, NitroxQuaternion rotation, NitroxVector3 cameraPosition, NitroxQuaternion cameraRotation, NitroxTechType techType, Optional<NitroxId> parentId, bool isFurniture, Optional<BuilderMetadata> rotationMetadata)
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

        public BasePiece(NitroxId id, NitroxVector3 itemPosition, NitroxQuaternion rotation, NitroxVector3 cameraPosition, NitroxQuaternion cameraRotation, NitroxTechType techType, Optional<NitroxId> parentId, bool isFurniture, Optional<BuilderMetadata> rotationMetadata, Optional<BasePieceMetadata> metadata) : this(id, itemPosition, rotation, cameraPosition, cameraRotation, techType, parentId, isFurniture, rotationMetadata)
        {
            Metadata = metadata;
        }

        public override string ToString()
        {
            return $"[BasePiece - ItemPosition: {ItemPosition}, Id: {Id}, Rotation: {Rotation}, CameraPosition: {CameraPosition}, CameraRotation: {CameraRotation}, TechType: {TechType}, ParentId: {ParentId}, ConstructionAmount: {ConstructionAmount}, IsFurniture: {IsFurniture}, BaseId: {BaseId}, RotationMetadata: {RotationMetadata}, BuildIndex: {BuildIndex}]";
        }
    }
}
