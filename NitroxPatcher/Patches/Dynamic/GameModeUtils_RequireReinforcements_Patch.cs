using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class GameModeUtils_RequireReinforcements_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(GameModeUtils);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("RequiresReinforcements", BindingFlags.Public | BindingFlags.Static);

        public static bool Prefix(ref bool __result)
        {
            return NitroxServiceLocator.LocateService<Building>().GameModeUtils_RequiresReinforcements_Pre(ref __result);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
