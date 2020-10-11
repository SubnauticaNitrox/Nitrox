using System;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel_Subnautica.Helper.Int3;
using ProtoBufNet;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata
{
    [Serializable]
    [ProtoContract]
    public class AnchoredFaceRotationMetadata : RotationMetadata
    {
        [ProtoMember(1)]
        public NitroxModel.DataStructures.Int3 Cell { get; set; }

        [ProtoMember(2)]
        public int Direction { get; set; }

        [ProtoMember(3)]
        public int FaceType { get; set; }
        
        protected AnchoredFaceRotationMetadata() : base(typeof(BaseAddFaceGhost))
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public AnchoredFaceRotationMetadata(Int3 cell, int facedirection, int facetype) : base(typeof(BaseAddFaceGhost))
        {
            Cell = cell.Model();
            Direction = facedirection;
            FaceType = facetype;
        }

        public override string ToString()
        {
            return "[AnchoredFaceRotationMetadata Cell: " + Cell + " Direction: " + Direction + " FaceType: " + FaceType + "]";
        }
    }
}
