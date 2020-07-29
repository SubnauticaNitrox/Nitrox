using System;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class BasePiece : NitroxBehavior
    {
        [ProtoMember(4)]
        public NitroxTechType TechType { get; set; }

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
            RotationMetadata = Optional.Empty;
            Metadata = Optional.Empty;
        }

        public BasePiece(NitroxVector3 cameraPosition, NitroxQuaternion cameraRotation, NitroxTechType techType, bool isFurniture, Optional<RotationMetadata> rotationMetadata)
        {
            TechType = techType;
            CameraPosition = cameraPosition;
            CameraRotation = cameraRotation;
            IsFurniture = isFurniture;
            ConstructionAmount = 0.0f;
            ConstructionCompleted = false;
            RotationMetadata = rotationMetadata;
            Metadata = Optional.Empty;
        }

        public override string ToString()
        {
            return "[BasePiece - Id: " + Id + " CameraPosition: " + CameraPosition + "CameraRotation: " + CameraRotation + " TechType: " + TechType + " ConstructionAmount: " + ConstructionAmount + " IsFurniture: " + IsFurniture + " BaseId: " + BaseId + " RotationMetadata: " + RotationMetadata + "]";
        }
    }
}
