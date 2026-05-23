using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.CinematicController;
using NitroxClient.Unity.Helper;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class PlayerCinematicController_OnPlayerCinematicModeEnd_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((PlayerCinematicController t) => t.OnPlayerCinematicModeEnd());

    public static void Prefix(PlayerCinematicController __instance)
    {
        if (!__instance.cinematicModeActive)
        {
            return;
        }

        if (!__instance.TryGetComponentInParent(out NitroxEntity entity, true))
        {
            Log.Warn($"[CinematicLock] OnEnd: No NitroxEntity for \"{__instance.gameObject.GetFullHierarchyPath()}\" found!");
            return;
        }

        Log.Info($"[CinematicLock] Ending cinematic:");
        Log.Info($"  - GameObject: {__instance.gameObject.name}");
        Log.Info($"  - Entity ID: {entity.Id}");
        Log.Info($"  - Animation: {__instance.playerViewAnimationName}");

        int identifier = __instance.gameObject.GetHierarchyPath(entity.gameObject).GetHashCode();
        
        // Broadcast cinematic end
        Resolve<PlayerCinematics>().EndCinematicMode(Resolve<LocalPlayer>().SessionId.Value, entity.Id, identifier, __instance.playerViewAnimationName);
        
        // Release the exclusive lock so other players can use this cinematic
        Log.Info($"[CinematicLock] Releasing lock (downgrading to TRANSIENT) for entity {entity.Id}");
        Resolve<SimulationOwnership>().RequestSimulationLock(entity.Id, SimulationLockType.TRANSIENT);
        Log.Info($"[CinematicLock] Lock released, cinematic is now available for other players");
    }
}
