using ProtoBufNet;
using System;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel_Subnautica.Helper.Int3;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation
{
    [Serializable]
    [ProtoContract]
    public class BaseFaceRotationMetadata : RotationMetadata
    {
        public const long VERSION = 1;

        // Base modules anchor based on a face.  This can be constructed via these two attributes.
        [ProtoMember(1)]
        public NitroxModel.DataStructures.Int3 AnchoredFaceCell { get; set; }

        [ProtoMember(2)]
        public int AnchoredFaceDirection { get; set; }

        [ProtoMember(3)]
        public int AnchoredFaceType { get; set; }


        public BaseFaceRotationMetadata()
        {
            // For serialization purposes
        }

        public BaseFaceRotationMetadata(Int3 cell, int facedirection, int facetype)
        {
            AnchoredFaceCell = cell.Model();
            AnchoredFaceDirection = facedirection;
            AnchoredFaceType = facetype;
        }

        public override string ToString()
        {
            return "[BaseFaceRotationMetadata AnchoredFaceCell: " + AnchoredFaceCell + " AnchoredFaceDirection: " + AnchoredFaceDirection + " AnchoredFaceType: " + AnchoredFaceType + "]";
        }
    }
}
