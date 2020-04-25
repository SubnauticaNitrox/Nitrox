using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class EntityCell_QueueForAwake_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(EntityCell).GetMethod("QueueForAwake");

        public static bool Prefix(EntityCell __instance)
        {
            NitroxServiceLocator.LocateService<Terrain>().CellLoaded(__instance.BatchId, __instance.CellId, __instance.Level);

            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
