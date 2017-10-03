using Harmony;
using NitroxModel.Helper.GameLogic;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches.Server
{
    public class SwimBehaviour_SwimToInterval_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(SwimBehaviour);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SwimToInternal", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(SwimBehaviour __instance)
        {
            String guid = GuidHelper.GetGuid(__instance.gameObject);
            NitroxServer.Server.Logic.AI.CreatureMoved(guid);
            return true;
        }        

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
