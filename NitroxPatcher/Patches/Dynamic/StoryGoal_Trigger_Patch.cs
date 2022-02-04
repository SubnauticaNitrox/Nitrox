using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;
using Story;

namespace NitroxPatcher.Patches.Dynamic
{
    public class StoryGoal_Trigger_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((StoryGoal t) => t.Trigger());
        private static readonly List<string> auroraWarningKeys = new() { "Story_AuroraWarning1", "Story_AuroraWarning2", "Story_AuroraWarning3", "Story_AuroraWarning4" };

        // The aurora warnings are managed and sent by the server, so they should't be triggered by the client itself
        public static bool Prefix(StoryGoal __instance)
        {
            return !auroraWarningKeys.Contains(__instance.key);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
