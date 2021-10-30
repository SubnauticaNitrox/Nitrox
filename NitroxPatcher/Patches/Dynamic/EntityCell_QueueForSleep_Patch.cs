using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class EntityCell_QueueForSleep_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((EntityCell t) => t.QueueForSleep(default(IQueue<EntityCell>)));

        public static bool Prefix(EntityCell __instance)
        {
            NitroxServiceLocator.LocateService<Terrain>().CellUnloaded(__instance.BatchId, __instance.CellId, __instance.Level);
            return true;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
