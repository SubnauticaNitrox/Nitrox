using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata
{
    [Serializable]
    [DataContract]
    public class AnchoredFaceBuilderMetadata : BuilderMetadata
    {
        [DataMember(Order = 1)]
        public NitroxInt3 Cell { get; set; }

        [DataMember(Order = 2)]
        public int Direction { get; set; }

        [DataMember(Order = 3)]
        public int FaceType { get; set; }

        [DataMember(Order = 4)]
        public NitroxInt3 Anchor { get; set; }

        [IgnoreConstructor]
        protected AnchoredFaceBuilderMetadata()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public AnchoredFaceBuilderMetadata(NitroxInt3 cell, int direction, int faceType, NitroxInt3 anchor)
        {
            Cell = cell;
            Direction = direction;
            FaceType = faceType;
            Anchor = anchor;
        }

        public override string ToString()
        {
            return $"[AnchoredFaceBuilderMetadata - Cell: {Cell}, Direction: {Direction}, FaceType: {FaceType}, Anchor: {Anchor}]";
        }
    }
}
