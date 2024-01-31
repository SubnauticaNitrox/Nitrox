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
            UpdateCrashHomeOpen(crashHome);
        }
        else
        {
            Log.Error($"[{nameof(CrashHomeMetadataProcessor)}] Could not find {nameof(CrashHome)} on {gameObject}");
        }
    }

    public static void UpdateCrashHomeOpen(CrashHome crashHome)
    {
        // From CrashHome.Update
        // We also add a distance detection to take into account if the crash is still in the home or not
        bool isCrashResting = crashHome.crash && crashHome.crash.IsResting() && crashHome &&
            Vector3.Distance(crashHome.transform.position, crashHome.crash.transform.position) < 1f;
        crashHome.animator.SetBool(AnimatorHashID.attacking, !isCrashResting);
        crashHome.prevClosed = isCrashResting;
    }
}
