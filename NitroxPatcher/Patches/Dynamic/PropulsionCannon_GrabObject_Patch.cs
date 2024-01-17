using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Gui.HUD;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class PropulsionCannon_GrabObject_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PropulsionCannon t) => t.GrabObject(default(GameObject)));

    private static bool skipPrefixPatch;

    public static bool Prefix(PropulsionCannon __instance, GameObject target)
    {
        if (skipPrefixPatch)
        {
            return true;
        }

        if (!target.TryGetIdOrWarn(out NitroxId id))
        {
            return true;
        }

        if (Resolve<SimulationOwnership>().HasExclusiveLock(id))
        {
            Log.Debug($"Already have an exclusive lock on the grabbed propulsion cannon object: {id}");
            return true;
        }

        PropulsionGrab context = new(__instance, target);
        LockRequest<PropulsionGrab> lockRequest = new(id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);

        Resolve<SimulationOwnership>().RequestSimulationLock(lockRequest);

        return false;
    }

    private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAquired, PropulsionGrab context)
    {
        if (lockAquired)
        {
            EntityPositionBroadcaster.WatchEntity(id);

            skipPrefixPatch = true;
            context.Cannon.GrabObject(context.GrabbedObject);
            skipPrefixPatch = false;
        }
        else
        {
            context.GrabbedObject.AddComponent<DenyOwnershipHand>();
        }
    }
}
