using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class CyclopsLightingPanel_OnSubConstructionComplete_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(CyclopsLightingPanel).GetMethod("SubConstructionComplete", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(CyclopsLightingPanel __instance)
        {
            // Suppress powered on if a cyclops´s floodlight is set to false            
            GameObject gameObject = __instance.gameObject.transform.parent.gameObject; // GO = LightsControl, Parent = main cyclops game object
            NitroxId id = NitroxEntity.GetId(gameObject);
            Optional<CyclopsModel> model = NitroxServiceLocator.LocateService<Vehicles>().TryGetVehicle<CyclopsModel>(id);
            if (!model.HasValue)
            {
                Log.Error($"{nameof(CyclopsLightingPanel_OnSubConstructionComplete_Patch)}: Could not find {nameof(CyclopsModel)} by Nitrox id {id}.\nGO containing wrong id: {__instance.GetHierarchyPath()}");
                return false;
            }

            return model.Value.FloodLightsOn;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
