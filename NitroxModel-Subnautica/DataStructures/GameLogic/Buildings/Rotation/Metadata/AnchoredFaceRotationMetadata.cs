using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata
{
    [ZeroFormattable]
    [ProtoContract]
    public class AnchoredFaceRotationMetadata : RotationMetadata
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual NitroxInt3 Cell { get; set; }

        [Index(1)]
        [ProtoMember(2)]
        public virtual int Direction { get; set; }

        [Index(2)]
        [ProtoMember(3)]
        public virtual int FaceType { get; set; }

        public AnchoredFaceRotationMetadata() : base(typeof(BaseAddFaceGhost))
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public AnchoredFaceRotationMetadata(NitroxInt3 cell, int faceDirection, int faceType) : base(typeof(BaseAddFaceGhost))
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
