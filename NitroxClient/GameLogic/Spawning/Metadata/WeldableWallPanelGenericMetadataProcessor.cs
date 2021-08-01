using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata
{
    class WeldableWallPanelGenericMetadataProcessor : GenericEntityMetadataProcessor<WeldableWallPanelGenericMetadata>
    {
        public override void ProcessMetadata(GameObject gameObject, WeldableWallPanelGenericMetadata metadata)
        {
            WeldableWallPanelGeneric weldableWallPanelGeneric = gameObject.GetComponent<WeldableWallPanelGeneric>();
            weldableWallPanelGeneric.liveMixin.health = metadata.LiveMixInHealth;
        }
    }
}
