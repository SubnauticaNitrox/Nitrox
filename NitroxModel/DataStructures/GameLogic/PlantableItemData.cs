using System;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable]
[JsonContractTransition]
public class PlantableItemData : ItemData
{
    [JsonMemberTransition]
    public double PlantedGameTime { get; }

    /// <summary>
    /// Extends the basic ItemData by adding the game time when the Plantable was added to its Planter container.
    /// </summary>
    /// <inheritdoc cref="ItemData"/>
    /// <param name="plantedGameTime">Clients will use this to determine expected plant growth progress when connecting </param>
    public PlantableItemData(NitroxId containerId, NitroxId itemId, byte[] serializedData, double plantedGameTime) : base(containerId, itemId, serializedData)
    {
        PlantedGameTime = plantedGameTime;
    }

    public override string ToString()
    {
        return $"[PlantedItemData - {base.ToString()}, PlantedGameTime: {PlantedGameTime}]";
    }
}
