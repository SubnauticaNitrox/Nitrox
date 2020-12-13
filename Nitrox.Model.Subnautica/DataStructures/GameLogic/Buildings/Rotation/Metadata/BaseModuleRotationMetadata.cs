using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Buildings.Rotation;
using ProtoBufNet;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata
{
    [Serializable]
    [ProtoContract]
    public class BaseModuleRotationMetadata : RotationMetadata
    {
        // Base modules anchor based on a face.  This can be constructed via these two attributes.
        [ProtoMember(1)]
        public NitroxInt3 Cell { get; set; }

        [ProtoMember(2)]
        public int Direction { get; set; }

        protected BaseModuleRotationMetadata() : base(typeof(BaseAddModuleGhost))
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public BaseModuleRotationMetadata(Int3 cell, int direction) : base(typeof(BaseAddModuleGhost))
        {
            Cell = cell.ToDto();
            Direction = direction;
        }

        public override string ToString()
        {
            return $"[BaseModuleRotationMetadata - Cell: {Cell}, Direction: {Direction}]";
        }
    }
}
