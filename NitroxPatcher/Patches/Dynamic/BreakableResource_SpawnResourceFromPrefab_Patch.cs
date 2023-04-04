using HarmonyLib;
using NitroxModel.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NitroxPatcher.Patches.Dynamic;

internal class BreakableResource_SpawnResourceFromPrefab_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD_ORIGINAL = Reflect.Method((BreakableResource t) => t.SpawnResourceFromPrefab(default));
    public static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(TARGET_METHOD_ORIGINAL);


    public override void Patch(Harmony harmony)
    {
        PatchTranspiler(harmony, TARGET_METHOD);
    }
}
