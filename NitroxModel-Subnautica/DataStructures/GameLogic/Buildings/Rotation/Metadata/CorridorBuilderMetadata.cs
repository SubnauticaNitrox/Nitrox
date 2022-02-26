using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Unity;
using ProtoBufNet;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation
{
    [Serializable]
    [ProtoContract]
    public class CorridorBuilderMetadata : BuilderMetadata
    {
        // Corridor internally maintains an int rotation that is changed by the scroll wheel.
        // When the scroll wheel moves, the code mod 4's a counter to decide the next piece.
        [ProtoMember(1)]
        public int Rotation { get; set; }

        // Send Position data instead of relying on camera positioning.
        [ProtoMember(2)]
        public NitroxVector3 Position { get; set; }
        
        [ProtoMember(3)]
        public bool HasTargetBase { get; set; }
        
        [ProtoMember(4)]
        public NitroxInt3 Cell { get; set; }

        protected CorridorBuilderMetadata() : base(typeof(BaseAddCorridorGhost))
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public CorridorBuilderMetadata(NitroxVector3 position, int rotation, bool hasTargetBase, NitroxInt3 targetCell) : base(typeof(BaseAddCorridorGhost))
        {
            Position = position;
            Rotation = rotation;
            HasTargetBase = hasTargetBase;
            Cell = targetCell;
        }

        public override string ToString()
        {
            return $"[CorridorRotationMetadata - Rotation: {Rotation} ]";
        }
    }
}
