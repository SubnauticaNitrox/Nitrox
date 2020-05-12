using ProtoBufNet;
using System;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation
{
    [Serializable]
    [ProtoContract]
    public class BaseCorridorRotationMetadata : RotationMetadata
    {
        public const long VERSION = 2;

        // Corridor internally maintains an int rotation that is changed by the scroll wheel.
        // When the scroll wheel moves, the code mod 4's a counter to decide the next piece.
        [ProtoMember(1)]
        public int Rotation { get; set; }

        public BaseCorridorRotationMetadata() : base(typeof(BaseAddCorridorGhost))
        {
            // For serialization purposes
        }

        public BaseCorridorRotationMetadata(int rotation) : base (typeof(BaseAddCorridorGhost))
        {
            Rotation = rotation;
        }

        public override string ToString()
        {
            return "[BaseCorridorRotationMetadata Rotation: " + Rotation + " ]";
        }
    }
}
