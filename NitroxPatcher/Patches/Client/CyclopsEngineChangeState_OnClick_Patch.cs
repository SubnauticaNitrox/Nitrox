using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel.Helper.GameLogic;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches.Client
{
    public class CyclopsEngineChangeState_OnClick_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsEngineChangeState);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnClick", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(CyclopsEngineChangeState __instance)
        {
            String guid = GuidHelper.GetGuid(__instance.subRoot.gameObject);
            Multiplayer.Logic.Cyclops.TogglEngineState(guid, __instance.motorMode.engineOn, (bool)__instance.ReflectionGet("startEngine"));
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
