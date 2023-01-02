using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata
{
    [Serializable]
    [DataContract]
    public class CorridorBuilderMetadata : BuilderMetadata
    {
        // Corridor internally maintains an int rotation that is changed by the scroll wheel.
        // When the scroll wheel moves, the code mod 4's a counter to decide the next piece.
        [DataMember(Order = 1)]
        public int Rotation { get; set; }

        // Send Position data instead of relying on camera positioning.
        [DataMember(Order = 2)]
        public NitroxVector3 Position { get; set; }
        
        [DataMember(Order = 3)]
        public bool HasTargetBase { get; set; }
        
        [DataMember(Order = 4)]
        public NitroxInt3 Cell { get; set; }

        [IgnoreConstructor]
        protected CorridorBuilderMetadata()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public CorridorBuilderMetadata(NitroxVector3 position, int rotation, bool hasTargetBase, NitroxInt3 cell)
        {
            Position = position;
            Rotation = rotation;
            HasTargetBase = hasTargetBase;
            Cell = cell;
        }

        public override string ToString()
        {
            return $"[CorridorRotationMetadata - Rotation: {Rotation} ]";
        }
    }
}
