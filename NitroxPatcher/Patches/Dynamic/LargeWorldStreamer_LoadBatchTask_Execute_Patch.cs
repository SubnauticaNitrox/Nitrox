using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class LargeWorldStreamer_LoadBatchTask_Execute_Patch : NitroxPatch, IDynamicPatch
{
    public override MethodInfo targetMethod { get; } = Reflect.Method((LargeWorldStreamer.LoadBatchTask t) => t.Execute());

    public static void Prefix(BatchCells ___batchCells)
    {
        Resolve<Terrain>().BatchLoaded(___batchCells.batch);
    }
}
