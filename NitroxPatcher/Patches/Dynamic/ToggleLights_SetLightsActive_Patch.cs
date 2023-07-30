using System;
using System.Collections.Generic;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class ToggleLights_SetLightsActive_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((ToggleLights t) => t.SetLightsActive(default(bool)));

    private static readonly HashSet<Type> syncedParents = new()
    {
        typeof(SeaMoth),
        typeof(Seaglide),
        typeof(FlashLight),
        // LEDLight uses ToggleLights, but does not provide a method to toggle them.
        typeof(LEDLight)
    };

    public static bool Prefix(ToggleLights __instance, out bool __state)
    {
        __state = __instance.lightsActive;
        return true;
    }

    public static void Postfix(ToggleLights __instance, bool __state)
    {
        if (__state != __instance.lightsActive)
        {
            // Find the right gameobject in the hierarchy to sync on:
            GameObject gameObject = null;
            Type type = null;
            foreach (Type t in syncedParents)
            {
                if (__instance.GetComponent(t))
                {
                    type = t;
                    gameObject = __instance.gameObject;
                    break;
                }
                if (__instance.GetComponentInParent(t))
                {
                    type = t;
                    gameObject = __instance.transform.parent.gameObject;
                    break;
                }
            }

            if (!gameObject)
            {
                DebugUtils.PrintHierarchy(__instance.gameObject);
            }

            if (!gameObject.TryGetIdOrWarn(out NitroxId id))
            {
                return;
            }

            if (type == typeof(SeaMoth))
            {
                if (Resolve<SimulationOwnership>().HasAnyLockType(id))
                {
                    SeaMoth seamoth = gameObject.GetComponent<SeaMoth>();
                    Resolve<Entities>().EntityMetadataChanged(seamoth, id);
                }
            }
            else
            {
                Resolve<IPacketSender>().Send(new NitroxModel.Packets.ToggleLights(id, __instance.lightsActive));
            }
        }
    }
}
