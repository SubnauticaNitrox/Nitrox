using System;
using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

internal sealed partial class Battery_charge_set_Patch : NitroxPatch, IDynamicPatch
{
    private static Entities entities;
    public static readonly MethodInfo TARGET_METHOD = Reflect.Property((Battery t) => t.charge).SetMethod;

    public Battery_charge_set_Patch(Entities e)
    {
        entities = e ?? throw new ArgumentNullException(nameof(e));
    }

    public static void Prefix(Battery __instance, float value)
    {
        // Broadcast update only once per integer change
        if (Math.Abs(Math.Floor(__instance.charge) - Math.Floor(value)) > 0.0 &&
            __instance.TryGetIdOrWarn(out NitroxId id))
        {
            entities.EntityMetadataChanged(__instance, id);
        }
    }
}
