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
        // Base modules anchor based on a face.  This can be constructed via these two attributes.
        [ProtoMember(1)]
        public NitroxModel.DataStructures.Int3 Cell { get; set; }

        [ProtoMember(2)]
        public int Direction { get; set; }

        public BaseModuleRotationMetadata() : base(typeof(BaseAddModuleGhost))
        {
            // For serialization purposes
        }

        public BaseModuleRotationMetadata(Int3 cell, int direction) : base (typeof(BaseAddModuleGhost))
        {
            Cell = cell.Model();
            Direction = direction;
        }

        public override string ToString()
        {
            return "[BaseModuleRotationMetadata cell: " + Cell + " direction: " + Direction + " ]";
        }
    }
}
