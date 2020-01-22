using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    public class CyclopsLightingPanel_ToggleFloodlights_Patch : NitroxPatch, IDynamicPatch
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
                NitroxId id = NitroxIdentifier.GetId(__instance.cyclopsRoot.gameObject);
                NitroxServiceLocator.LocateService<Cyclops>().BroadcastToggleFloodLights(id, __instance.floodlightsOn);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
