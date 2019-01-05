using ProtoBuf;
using System;

namespace NitroxModel.DataStructures.GameLogic.Buildings.Rotation
{
    [Serializable]
    [ProtoContract]
    public class CorridorRotationMetadata : RotationMetadata
    {
        // Corridor internally maintains an int rotation that is changed by the scroll wheel.
        // When the scroll wheel moves, the code mod 4's a counter to decide the next piece.
        [ProtoMember(1)]
        public int Rotation { get; set; }

        public CorridorRotationMetadata() : base(typeof(BaseAddCorridorGhost))
        {
            // For serialization purposes
        }

        public CorridorRotationMetadata(int rotation) : base (typeof(BaseAddCorridorGhost))
        {
            Rotation = rotation;
        }

        public override string ToString()
        {
            return "[CorridorRotationMetadata Rotation: " + Rotation + " ]";
        }
    }
}
