using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class PrecursorDoorwayMetadataProcessor : EntityMetadataProcessor<PrecursorDoorwayMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, PrecursorDoorwayMetadata metadata)
    {
        Log.Info($"Received precursor door metadata change for {gameObject.name} with data of {metadata}");

        PrecursorDoorway precursorDoorway = gameObject.GetComponent<PrecursorDoorway>();
        precursorDoorway.isOpen = metadata.IsOpen;

        if (metadata.IsOpen)
        {
            precursorDoorway.BroadcastMessage("DisableField");
        }
        else
        {
            precursorDoorway.BroadcastMessage("EnableField");
        }
    }
}
