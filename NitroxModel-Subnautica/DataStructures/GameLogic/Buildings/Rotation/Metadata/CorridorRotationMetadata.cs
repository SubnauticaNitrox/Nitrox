using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation
{
    [ZeroFormattable]
    [ProtoContract]
    public class CorridorRotationMetadata : RotationMetadata
    {
        // Corridor internally maintains an int rotation that is changed by the scroll wheel.
        // When the scroll wheel moves, the code mod 4's a counter to decide the next piece.
        [Index(0)]
        [ProtoMember(1)]
        public virtual int Rotation { get; set; }

        public CorridorRotationMetadata() : base(typeof(BaseAddCorridorGhost))
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public CorridorRotationMetadata(int rotation) : base(typeof(BaseAddCorridorGhost))
        {
            Rotation = rotation;
        }

        public override string ToString()
        {
            return $"[CorridorRotationMetadata - Rotation: {Rotation} ]";
        }
    }
}
