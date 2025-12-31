using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class GenericConsoleMetadataProcessor : EntityMetadataProcessor<GenericConsoleMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, GenericConsoleMetadata metadata)
    {
        GenericConsole console = gameObject.GetComponent<GenericConsole>();
        if (console && !console.gotUsed && metadata.GotUsed)
        {
            console.OnStoryHandTarget();
        }
    }
}
