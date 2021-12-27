using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic
{
    [ZeroFormattable]
    [ProtoContract]
    public class BasePiece
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual NitroxId Id { get; set; }

        [Index(1)]
        [ProtoMember(2)]
        public virtual NitroxVector3 ItemPosition { get; set; }

        [Index(2)]
        [ProtoMember(3)]
        public virtual NitroxQuaternion Rotation { get; set; }

        [Index(3)]
        [ProtoMember(4)]
        public virtual NitroxTechType TechType { get; set; }

        [ProtoMember(5, DynamicType = true)]
        [Index(4)]
        public virtual Optional<NitroxId> ParentId { get; set; }

        [ProtoMember(6)]
        [Index(5)]
        public virtual NitroxVector3 CameraPosition { get; set; }

        [ProtoMember(7)]
        [Index(6)]
        public virtual NitroxQuaternion CameraRotation { get; set; }

        [ProtoMember(8)]
        [Index(7)]
        public virtual float ConstructionAmount { get; set; }

        [ProtoMember(9)]
        [Index(8)]
        public virtual bool ConstructionCompleted { get; set; }

        [ProtoMember(10)]
        [Index(9)]
        public virtual bool IsFurniture { get; set; }

        [ProtoMember(11)]
        [Index(10)]
        public virtual NitroxId BaseId { get; set; }

        [ProtoMember(12, DynamicType = true)]
        [Index(11)]
        public virtual Optional<RotationMetadata> RotationMetadata { get; set; }

        [ProtoMember(13, DynamicType = true)]
        [Index(12)]
        public virtual Optional<BasePieceMetadata> Metadata { get; set; }

        [ProtoMember(14, DynamicType = true)]
        [Index(13)]
        public virtual int BuildIndex { get; set; }

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

        public BasePiece(NitroxId id, NitroxVector3 itemPosition, NitroxQuaternion rotation, NitroxVector3 cameraPosition, NitroxQuaternion cameraRotation, NitroxTechType techType, Optional<NitroxId> parentId, bool isFurniture, Optional<RotationMetadata> rotationMetadata, Optional<BasePieceMetadata> metadata) : this(id, itemPosition, rotation, cameraPosition, cameraRotation, techType, parentId, isFurniture, rotationMetadata)
        {
            Metadata = metadata;
        }

        public override string ToString()
        {
            return $"[BasePiece - ItemPosition: {ItemPosition}, Id: {Id}, Rotation: {Rotation}, CameraPosition: {CameraPosition}, CameraRotation: {CameraRotation}, TechType: {TechType}, ParentId: {ParentId}, ConstructionAmount: {ConstructionAmount}, IsFurniture: {IsFurniture}, BaseId: {BaseId}, RotationMetadata: {RotationMetadata}, BuildIndex: {BuildIndex}]";
        }
    }
}
