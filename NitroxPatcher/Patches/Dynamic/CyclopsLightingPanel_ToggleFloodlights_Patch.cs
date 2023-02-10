using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class CyclopsLightingPanel_ToggleFloodlights_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsLightingPanel t) => t.ToggleFloodlights());

        public static bool Prefix(CyclopsLightingPanel __instance, out bool __state)
        {
            __state = __instance.floodlightsOn;
            return true;
        }

        public static void Postfix(CyclopsLightingPanel __instance, bool __state)
        {
            if (__state != __instance.floodlightsOn)
            {
                NitroxId id = NitroxEntity.GetId(__instance.gameObject);
                Resolve<Entities>().EntityMetadataChanged(__instance, id);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, prefix:true, postfix:true);
        }
    }
}
