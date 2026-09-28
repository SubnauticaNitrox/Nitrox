using System.Collections;
using NitroxClient.GameLogic.InitialSync.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.CinematicController;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync;

/// <summary>
/// A player already asleep when we join never sends us their start packet, so their bed pose/position is remade here from whichever bed-side lock they hold.
/// </summary>
public sealed class BedSleepInitialSyncProcessor : InitialSyncProcessor
{
    private readonly PlayerManager remotePlayerManager;

    public BedSleepInitialSyncProcessor(PlayerManager remotePlayerManager)
    {
        this.remotePlayerManager = remotePlayerManager;

        AddDependency<GlobalRootInitialSyncProcessor>();
        AddDependency<RemotePlayerInitialSyncProcessor>();
    }

    public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
    {
        Bed[] beds = Object.FindObjectsOfType<Bed>();
        foreach (SimulatedEntity simulatedEntity in packet.InitialSimulationOwnerships)
        {
            if (!remotePlayerManager.TryFind(simulatedEntity.SessionId, out RemotePlayer remotePlayer) ||
                !remotePlayer.Body || !remotePlayer.AnimationController ||
                !TryFindSleepingSide(beds, simulatedEntity.Id, out Bed bed, out PlayerCinematicController controller))
            {
                continue;
            }

            remotePlayer.AnimationController["cinematics_enabled"] = true;
            remotePlayer.AnimationController[controller.playerViewAnimationName] = true;
            remotePlayer.SetInCinematic(simulatedEntity.Id);
            bed.gameObject.EnsureComponent<BedRemotePositioner>().Begin(remotePlayer, controller);
        }

        yield break;
    }

    private static bool TryFindSleepingSide(Bed[] beds, NitroxId lockId, out Bed bed, out PlayerCinematicController controller)
    {
        foreach (Bed candidate in beds)
        {
            if (!candidate.gameObject.TryGetComponent(out NitroxEntity bedEntity))
            {
                continue;
            }

            if (lockId == BedLockId.Resolve(bedEntity, candidate.leftLieDownCinematicController))
            {
                bed = candidate;
                controller = candidate.leftLieDownCinematicController;
                return true;
            }

            if (lockId == BedLockId.Resolve(bedEntity, candidate.rightLieDownCinematicController))
            {
                bed = candidate;
                controller = candidate.rightLieDownCinematicController;
                return true;
            }
        }

        bed = null;
        controller = null;
        return false;
    }
}
