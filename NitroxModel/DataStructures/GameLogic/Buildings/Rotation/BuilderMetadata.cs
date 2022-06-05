using System;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Buildings.Rotation;

[Serializable]
[JsonContractTransition]
public abstract class BuilderMetadata
{
    public Type GhostType { get; set; }

    public BuilderMetadata(Type ghostType)
    {
        GhostType = ghostType;
    }
}
