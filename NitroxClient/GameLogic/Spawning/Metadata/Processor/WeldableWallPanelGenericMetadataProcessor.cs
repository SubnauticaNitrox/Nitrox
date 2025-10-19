using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class WeldableWallPanelGenericMetadataProcessor : EntityMetadataProcessor<WeldableWallPanelGenericMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, WeldableWallPanelGenericMetadata metadata)
    {
        WeldableWallPanelGeneric weldableWallPanelGeneric = gameObject.GetComponent<WeldableWallPanelGeneric>();
        weldableWallPanelGeneric.liveMixin.health = metadata.LiveMixInHealth;
    }
}
