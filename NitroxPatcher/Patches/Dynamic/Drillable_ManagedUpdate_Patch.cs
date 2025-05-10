using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxPatcher.PatternMatching;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic;

// We need to suppress the EntitySpawnedByClient packet from the UnsafeAdd call since it doesn't quite handle this case correctly
public sealed class Drillable_ManagedUpdate_Patch : PacketSuppressorPatch<EntitySpawnedByClient>, IDynamicPatch
{
    // Additional field necessary to appease the unit tests
    public static readonly MethodInfo targetMethod = Reflect.Method((Drillable t) => t.ManagedUpdate());
    public override MethodInfo TARGET_METHOD => targetMethod;

    private static readonly InstructionsPattern pattern = new()
    {
        { Reflect.Method((ItemsContainer c) => c.UnsafeAdd(default)), "UnsafeAdd" }
    };

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return instructions.InsertAfterMarker(pattern, "UnsafeAdd", [
            new(Ldarg_0),
            new(Ldloc_S, 4),
            new(Call, ((Action<Drillable, Pickupable>)Callback).Method)
        ]);
    }

    private static void Callback(Drillable drillable, Pickupable pickupable)
    {
        if (drillable.drillingExo.TryGetIdOrWarn(out NitroxId exosuitId))
        {
            Resolve<Items>().PickedUp(pickupable.gameObject, pickupable.GetTechType(), exosuitId);
        }
    }

    public override void Patch(Harmony harmony)
    {
        base.Patch(harmony);
        PatchTranspiler(harmony, TARGET_METHOD, ((Func<IEnumerable<CodeInstruction>, IEnumerable<CodeInstruction>>)Transpiler).Method);
    }
}
