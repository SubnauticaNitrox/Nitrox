using Harmony;
using System;
using System.Reflection;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches
{
    public class EntityCell_QueueForSleep_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(EntityCell);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("QueueForSleep");
        
        public static bool Prefix(EntityCell __instance)
        {
            Multiplayer.Logic.Terrain.CellUnloaded(__instance.BatchId, __instance.CellId, __instance.Level);
            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
