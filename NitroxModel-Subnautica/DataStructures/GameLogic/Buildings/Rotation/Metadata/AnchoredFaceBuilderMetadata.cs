using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using ProtoBufNet;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata
{
    [Serializable]
    [ProtoContract]
    public class AnchoredFaceBuilderMetadata : BuilderMetadata
    {
        [ProtoMember(1)]
        public NitroxInt3 Cell { get; set; }

        [ProtoMember(2)]
        public int Direction { get; set; }

        [ProtoMember(3)]
        public int FaceType { get; set; }

        protected AnchoredFaceBuilderMetadata() : base(typeof(BaseAddFaceGhost))
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public AnchoredFaceBuilderMetadata(NitroxInt3 cell, int faceDirection, int faceType) : base(typeof(BaseAddFaceGhost))
        {
            Cell = cell;
            Direction = faceDirection;
            FaceType = faceType;
        }

        public override string ToString()
        {
            return $"[AnchoredFaceRotationMetadata - Cell: {Cell}, Direction: {Direction}, FaceType: {FaceType}]";
        }
    }
}
