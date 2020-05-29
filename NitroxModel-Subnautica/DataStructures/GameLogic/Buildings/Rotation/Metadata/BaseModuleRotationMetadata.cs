using ProtoBufNet;
using System;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel_Subnautica.Helper.Int3;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation
{
    [Serializable]
    [ProtoContract]
    public class BaseModuleRotationMetadata : RotationMetadata
    {
        public const long VERSION = 2;

        // Base modules anchor based on a face.  This can be constructed via these two attributes.
        [ProtoMember(1)]
        public NitroxModel.DataStructures.Int3 AnchoredFaceCell { get; set; }

        [ProtoMember(2)]
        public int AnchoredFaceDirection { get; set; }

        [ProtoMember(3)]
        public int AnchoredFaceType { get; set; }

        [ProtoMember(4)]
        public int ModuleDirection { get; set; }

        public BaseModuleRotationMetadata()
        {
            // For serialization purposes
        }

        public BaseModuleRotationMetadata(Int3 cell, int facedirection, int facetype, int moduledirection)
        {
            AnchoredFaceCell = cell.Model();
            AnchoredFaceDirection = facedirection;
            AnchoredFaceType = facetype;
            ModuleDirection = moduledirection;
        }

        public override string ToString()
        {
            return "[BaseModuleRotationMetadata AnchoredFaceCell: " + AnchoredFaceCell + " AnchoredFaceDirection: " + AnchoredFaceDirection + " AnchoredFaceType: " + AnchoredFaceType + " ModuleDirection: " + ModuleDirection + "]";
        }
    }
}
