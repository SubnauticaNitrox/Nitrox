using System;
using System.Collections.Generic;
using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CyclopsExternalDamageManager_CreatePoint_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsExternalDamageManager t) => t.CreatePoint());

    public static bool Prefix(CyclopsExternalDamageManager __instance, out (bool, List<CyclopsDamagePoint>) __state)
    {
        // Block from creating points if they aren't the owner of the sub
        bool hasLock = __instance.subRoot.TryGetNitroxId(out NitroxId id) && Resolve<SimulationOwnership>().HasAnyLockType(id);

        // Save the current damage state so we can find what changed in the postfix
        __state = (hasLock, [.. __instance.unusedDamagePoints]);

        return hasLock;
    }

    public static void Postfix(CyclopsExternalDamageManager __instance, (bool, List<CyclopsDamagePoint>) __state)
    {
        (bool hasLock, List<CyclopsDamagePoint> damagePoints) = __state;
        if (hasLock)
        {
            foreach (CyclopsDamagePoint damagePoint in damagePoints)
            {
                if (!__instance.unusedDamagePoints.Contains(damagePoint))
                {
                    int index = Array.IndexOf(__instance.damagePoints, damagePoint);
                    Resolve<Cyclops>().OnCreateDamagePoint(__instance.subRoot, index);
                    return;
                }
            }
        }
    }
}
