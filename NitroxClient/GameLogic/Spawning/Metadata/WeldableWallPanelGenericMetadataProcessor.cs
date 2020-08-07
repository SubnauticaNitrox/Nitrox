using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata
{
    class WeldableWallPanelGenericMetadataProcessor : GenericEntityMetadataProcessor<WeldableWallPanelGenericMetadata>
    {
        public override void ProcessMetadata(GameObject gameObject, WeldableWallPanelGenericMetadata metadata)
        {
            Log.Info($"Received weldable wall panel metadata change for {gameObject.name} with data of {metadata}");

            WeldableWallPanelGeneric weldableWallPanelGeneric = gameObject.GetComponent<WeldableWallPanelGeneric>();
            weldableWallPanelGeneric.liveMixin.health = metadata.LiveMixInHealth;
        }
    }
}
