using Harmony;
using NitroxClient.MonoBehaviours;
using NitroxClient.GameLogic.Helper;
using System;
using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches
{
    public class CyclopsLightingPanel_ToggleInternalLighting_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsLightingPanel);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("ToggleInternalLighting", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(CyclopsLightingPanel __instance, out bool __state)
        {
            __state = __instance.lightingOn;
            return true;
        }

        public static void Postfix(CyclopsLightingPanel __instance, bool __state)
        {
            if (__state != __instance.lightingOn)
            {
                string guid = GuidHelper.GetGuid(__instance.cyclopsRoot.gameObject);
                NitroxServiceLocator.LocateService<Cyclops>().ToggleInternalLight(guid, __instance.lightingOn);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
