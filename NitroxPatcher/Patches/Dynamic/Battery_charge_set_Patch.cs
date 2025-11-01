using System;
using System.Reflection;
using NitroxClient.GameLogic;
using Nitrox.Model.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Battery_charge_set_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Property((Battery t) => t.charge).SetMethod;

    public static void Prefix(Battery __instance, float value)
    {
        // Broadcast update only once per integer change
        if (Math.Abs(Math.Floor(__instance.charge) - Math.Floor(value)) > 0.0 &&
            __instance.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<Entities>().EntityMetadataChanged(__instance, id);
        }
    }
}
