using Harmony;
using NitroxClient.MonoBehaviours;
using NitroxClient.GameLogic.Helper;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class CyclopsLightingPanel_ToggleFloodlights_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsLightingPanel);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("ToggleFloodlights", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(CyclopsLightingPanel __instance, out bool __state)
        {
            __state = __instance.floodlightsOn;
            return true;
        }

        public static void Postfix(CyclopsLightingPanel __instance, bool __state)
        {
            if (__state != __instance.floodlightsOn)
            {
                String guid = GuidHelper.GetGuid(__instance.cyclopsRoot.gameObject);
                Multiplayer.Logic.Cyclops.ToggleFloodLights(guid, __instance.floodlightsOn);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
