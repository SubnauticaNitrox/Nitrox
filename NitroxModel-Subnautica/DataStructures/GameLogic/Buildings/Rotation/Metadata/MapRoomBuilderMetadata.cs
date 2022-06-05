using System;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.Serialization;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation;

[Serializable]
[JsonContractTransition]
public class MapRoomBuilderMetadata : BuilderMetadata
{
    // The map room internally maintains a cellType and connectionMask for rotation - these values
    // are updated when the scroll wheel changes.
    [JsonMemberTransition]
    public byte CellType { get; set; }

    [JsonMemberTransition]
    public int ConnectionMask { get; set; }

    public MapRoomBuilderMetadata(byte cellType, int connectionMask) : base(typeof(BaseAddMapRoomGhost))
    {
        CellType = cellType;
        ConnectionMask = connectionMask;
    }

    public override string ToString()
    {
        return $"[MapRoomRotationMetadata - CellType: {CellType}, ConnectionMask: {ConnectionMask}]";
    }
}
