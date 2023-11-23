using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class LargeWorldStreamer_LoadBatchTask_Execute_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((LargeWorldStreamer.LoadBatchTask t) => t.Execute());

    public static void Prefix(BatchCells ___batchCells)
    {
        Resolve<Terrain>().BatchLoaded(___batchCells.batch);
    }
}
