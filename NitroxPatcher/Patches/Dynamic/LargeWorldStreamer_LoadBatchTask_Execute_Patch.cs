using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class LargeWorldStreamer_LoadBatchTask_Execute_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((LargeWorldStreamer.LoadBatchTask t) => t.Execute());

    public static void Prefix(BatchCells ___batchCells)
    {
        Resolve<Terrain>().BatchLoaded(___batchCells.batch);
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
