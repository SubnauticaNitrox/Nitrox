using NitroxClient.Communication;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class BeaconMetadataProcessor : GenericEntityMetadataProcessor<BeaconMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, BeaconMetadata metadata)
    {
        if (gameObject.TryGetComponent(out Beacon beacon))
        {
            using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
            {
                // Must set Beacon.label so that Beacon.Start() doesn't overwrite Beacon.beaconLabel's label
                beacon.label = metadata.Label;
                beacon.beaconLabel.SetLabel(metadata.Label);
            }
        }
    }
}
