using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using NitroxPatcher.PatternMatching;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Drillable_ManagedUpdate_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Drillable t) => t.ManagedUpdate());

    private static readonly InstructionsPattern Pattern = new()
    {
        { Reflect.Method((ItemsContainer c) => c.UnsafeAdd(default)), "UnsafeAdd" }
    };

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return instructions.InsertAfterMarker(Pattern, "UnsafeAdd", [
            new(Ldarg_0),
            new(Ldloc_S, 4),
            new(Call, ((Action<Drillable, Pickupable>)Callback).Method)
        ]);
    }

    private static void Callback(Drillable drillable, Pickupable pickupable)
    {
        Resolve<Items>().PickedUp(pickupable.gameObject, pickupable.GetTechType(), drillable.drillingExo.storageContainer.transform);
    }
}
