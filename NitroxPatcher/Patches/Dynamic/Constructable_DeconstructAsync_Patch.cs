using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;
using NitroxPatcher.PatternMatching;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Constructable_DeconstructAsync_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((Constructable t) => t.DeconstructAsync(default, default)));

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions) =>
        instructions.RewriteOnPattern(
        [
            Ldc_I4_0,
            Ret,
            Ldloc_1,
            Reflect.Method((Constructable c) => c.UpdateMaterial()),
            [
                Ldloc_1,
                Ldc_I4_0, // False for "constructing"
                Reflect.Method(() => Constructable_Construct_Patch.ConstructionAmountModified(default, default))
            ]
        ]);
}
