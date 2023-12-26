using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class CrashHomeMetadataProcessor : EntityMetadataProcessor<CrashHomeMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, CrashHomeMetadata metadata)
    {
        if (gameObject.TryGetComponent(out CrashHome crashHome))
        {
            crashHome.spawnTime = metadata.SpawnTime;
            // Opens the crash once it has already spawned a crash
            crashHome.animator.SetBool(AnimatorHashID.attacking, crashHome.spawnTime > 0);
        }
        else
        {
            Log.Error($"[{nameof(CrashHomeMetadataProcessor)}] Couldn't find {nameof(CrashHome)} on {gameObject}");
        }
    }
}
