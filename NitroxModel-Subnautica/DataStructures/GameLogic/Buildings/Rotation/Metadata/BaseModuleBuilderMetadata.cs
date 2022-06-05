using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.Serialization;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation;

[Serializable]
[JsonContractTransition]
public class BaseModuleBuilderMetadata : BuilderMetadata
{
    // Base modules anchor based on a face.  This can be constructed via these two attributes.
    [JsonMemberTransition]
    public NitroxInt3 Cell { get; set; }

    [JsonMemberTransition]
    public int Direction { get; set; }

    public BaseModuleBuilderMetadata(NitroxInt3 cell, int direction) : base(typeof(BaseAddModuleGhost))
    {
        Cell = cell;
        Direction = direction;
    }

    public override string ToString()
    {
        return $"[BaseModuleRotationMetadata - Cell: {Cell}, Direction: {Direction}]";
    }
}
