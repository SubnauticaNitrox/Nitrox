using System;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[JsonContractTransition]
public class WeldableWallPanelGenericMetadata : EntityMetadata
{
    [JsonMemberTransition]
    public float LiveMixInHealth { get; }

    public WeldableWallPanelGenericMetadata(float liveMixInHealth)
    {
        LiveMixInHealth = liveMixInHealth;
    }

    public override string ToString()
    {
        return $"[WeldableWallPanelGenericMetadata - LiveMixInHealth: {LiveMixInHealth}]";
    }
}
