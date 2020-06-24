using ProtoBufNet;
using System;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel_Subnautica.Helper.Int3;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation
{
    [Serializable]
    [ProtoContract]
    public class FaceRotationMetadata : RotationMetadata
    {
        public const long VERSION = 1;

        // Base modules anchor based on a face. 
        [ProtoMember(1)]
        public NitroxModel.DataStructures.Int3 AnchoredFaceCell { get; set; }

        [ProtoMember(2)]
        public int AnchoredFaceDirection { get; set; }

        [ProtoMember(3)]
        public int AnchoredFaceType { get; set; }


        public FaceRotationMetadata() : base(typeof(BaseAddFaceGhost))
        {
            // For serialization purposes
        }

        public FaceRotationMetadata(Int3 cell, int facedirection, int facetype) : base(typeof(BaseAddFaceGhost))
        {
            AnchoredFaceCell = cell.Model();
            AnchoredFaceDirection = facedirection;
            AnchoredFaceType = facetype;
        }

        public override string ToString()
        {
            return "[FaceRotationMetadata AnchoredFaceCell: " + AnchoredFaceCell + " AnchoredFaceDirection: " + AnchoredFaceDirection + " AnchoredFaceType: " + AnchoredFaceType + "]";
        }
    }
}
