using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class EntityCell_QueueForAwake_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((EntityCell t) => t.QueueForAwake(default(IQueue<EntityCell>)));

    public static void Prefix(EntityCell __instance)
    {
        Resolve<Terrain>().CellLoaded(__instance.BatchId, __instance.CellId, __instance.Level);
    }
}
