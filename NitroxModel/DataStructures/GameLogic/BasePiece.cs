using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.DataStructures.GameLogic
{
    /// <summary>
    ///     Represents a piece of a player's base. E.g. a window, entry/exit hatch, multi-purpose room.
    ///     The <see cref="TechType"/> says what kind of base piece this is.
    /// </summary>
    [Serializable]
    [DataContract]
    public class BasePiece
    {
        [DataMember(Order = 1)]
        public NitroxId Id { get; set; }

        [DataMember(Order = 2)]
        public NitroxVector3 ItemPosition { get; set; }

        [DataMember(Order = 3)]
        public NitroxQuaternion Rotation { get; set; }

        [DataMember(Order = 4)]
        public NitroxTechType TechType { get; set; }

        [DataMember(Order = 5)]
        public Optional<NitroxId> ParentId { get; set; }

        [DataMember(Order = 6)]
        public NitroxVector3 CameraPosition { get; set; }

        [DataMember(Order = 7)]
        public NitroxQuaternion CameraRotation { get; set; }

        [DataMember(Order = 8)]
        public float ConstructionAmount { get; set; }

        [DataMember(Order = 9)]
        public bool ConstructionCompleted { get; set; }

        [DataMember(Order = 10)]
        public bool IsFurniture { get; set; }

        [DataMember(Order = 11)]
        public NitroxId BaseId { get; set; }

        [DataMember(Order = 12)]
        public Optional<BuilderMetadata> RotationMetadata { get; set; }

        [DataMember(Order = 13)]
        public Optional<BasePieceMetadata> Metadata { get; set; }

        [DataMember(Order = 14)]
        public int BuildIndex { get; set; }

        [IgnoreConstructor]
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

        /// <remarks>Used for deserialization</remarks>
        public BasePiece(
            NitroxId id,
            NitroxVector3 itemPosition,
            NitroxQuaternion rotation,
            NitroxTechType techType,
            Optional<NitroxId> parentId,
            NitroxVector3 cameraPosition,
            NitroxQuaternion cameraRotation,
            float constructionAmount,
            bool constructionCompleted,
            bool isFurniture,
            NitroxId baseId,
            Optional<BuilderMetadata> rotationMetadata,
            Optional<BasePieceMetadata> metadata,
            int buildIndex)
        {
            Id = id;
            ItemPosition = itemPosition;
            Rotation = rotation;
            TechType = techType;
            ParentId = parentId;
            CameraPosition = cameraPosition;
            CameraRotation = cameraRotation;
            ConstructionAmount = constructionAmount;
            ConstructionCompleted = constructionCompleted;
            IsFurniture = isFurniture;
            BaseId = baseId;
            RotationMetadata = rotationMetadata;
            Metadata = metadata;
            BuildIndex = buildIndex;
        }

        public override string ToString()
        {
            return $"[BasePiece - ItemPosition: {ItemPosition}, Id: {Id}, Rotation: {Rotation}, CameraPosition: {CameraPosition}, CameraRotation: {CameraRotation}, TechType: {TechType}, ParentId: {ParentId}, ConstructionAmount: {ConstructionAmount}, IsFurniture: {IsFurniture}, BaseId: {BaseId}, RotationMetadata: {RotationMetadata}, BuildIndex: {BuildIndex}]";
        }
    }
}
