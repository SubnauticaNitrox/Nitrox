using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation
{
    [ZeroFormattable]
    [ProtoContract]
    public class BaseModuleRotationMetadata : RotationMetadata
    {
        // Base modules anchor based on a face.  This can be constructed via these two attributes.
        [Index(0)]
        [ProtoMember(1)]
        public virtual NitroxInt3 Cell { get; set; }

        [Index(1)]
        [ProtoMember(2)]
        public virtual int Direction { get; set; }

        public BaseModuleRotationMetadata() : base(typeof(BaseAddModuleGhost))
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public BaseModuleRotationMetadata(NitroxInt3 cell, int direction) : base(typeof(BaseAddModuleGhost))
        {
            Cell = cell;
            Direction = direction;
        }

        public override string ToString()
        {
            return $"[BaseModuleRotationMetadata - Cell: {Cell}, Direction: {Direction}]";
        }
    }
}
