using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class EntityCell_QueueForAwake_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(EntityCell);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("QueueForAwake");

        public static bool Prefix(EntityCell __instance)
        {
            NitroxServiceLocator.LocateService<Terrain>().CellLoaded(__instance.BatchId, __instance.CellId, __instance.Level);

            return true;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
