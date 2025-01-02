using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours.Cyclops;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts the cyclops destruction, and safely removes every player from it. Also broadcasts the creation of the beacon.
/// </summary>
public sealed partial class CyclopsDestructionEvent_DestroyCyclops_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsDestructionEvent t) => t.DestroyCyclops());

    public static void Prefix(CyclopsDestructionEvent __instance)
    {
        bool wasInCyclops = Player.main.currentSub == __instance.subRoot;

        // Before the cyclops destruction, we move out the remote players so that they aren't stuck in its hierarchy
        if (__instance.subRoot && __instance.subRoot.TryGetComponent(out NitroxCyclops nitroxCyclops))
        {
            nitroxCyclops.RemoveAllPlayers();
        }

        if (wasInCyclops)
        {
            // Particular case here, this is not broadcasted and should not be, it's just there to have player be really inside the cyclops while not being registered by NitroxCyclops
            Player.main._currentSub = __instance.subRoot;
        }

        __instance.subLiveMixin.Kill();
    }

    public static void Postfix(CyclopsDestructionEvent __instance)
    {
        if (__instance.TryGetNitroxId(out NitroxId nitroxId))
        {
            Resolve<Vehicles>().BroadcastDestroyedCyclops(__instance.gameObject, nitroxId);
        }
    }

    /*
     * ADD at the end of the method:
     * CyclopsDestructionEvent_DestroyCyclops_Patch.ManageBeacon(component, this);
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).End() // Move before Ret
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Ldloc_2),
                                                new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => ManageBeacon(default, default)))
                                            ]).InstructionEnumeration();
    }

    public static void ManageBeacon(Beacon beacon, CyclopsDestructionEvent cyclopsDestructionEvent)
    {
        if (!cyclopsDestructionEvent.TryGetNitroxId(out NitroxId nitroxId))
        {
            return;
        }

        // We let the simulating player spawn it for everyone
        if (!Resolve<SimulationOwnership>().HasAnyLockType(nitroxId))
        {
            Object.Destroy(beacon.gameObject);
            return;
        }

        // We need to force this state for beaconLabel to wear the correct tag
        beacon.Start();
        Resolve<Items>().Dropped(beacon.gameObject, TechType.Beacon);
    }
}
