using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Serialization;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation;

[Serializable]
[JsonContractTransition]
public class CorridorBuilderMetadata : BuilderMetadata
{
    // Send Position data instead of relying on camera positioning.
    [JsonMemberTransition]
    public NitroxVector3 Position { get; set; }
        
    // Corridor internally maintains an int rotation that is changed by the scroll wheel.
    // When the scroll wheel moves, the code mod 4's a counter to decide the next piece.
    [JsonMemberTransition]
    public int Rotation { get; set; }

    [JsonMemberTransition]
    public bool HasTargetBase { get; set; }
        
    [JsonMemberTransition]
    public NitroxInt3 Cell { get; set; }

    public CorridorBuilderMetadata(NitroxVector3 position, int rotation, bool hasTargetBase, NitroxInt3 cell) : base(typeof(BaseAddCorridorGhost))
    {
        Position = position;
        Rotation = rotation;
        HasTargetBase = hasTargetBase;
        Cell = cell;
    }
    
    public override string ToString()
    {
        return $"[CorridorBuilderMetadata - Position: {Position}, Rotation: {Rotation}, HasTargetBase: {HasTargetBase}, Cell: {Cell}]";
    }
}
