using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    class CyclopsLightingPanel_OnSubConstructionComplete_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsLightingPanel t) => t.SubConstructionComplete());

        public static bool Prefix(CyclopsLightingPanel __instance)
        {
            // Suppress powered on if a cyclops´s floodlight is set to false            
            GameObject gameObject = __instance.gameObject.transform.parent.gameObject; // GO = LightsControl, Parent = main cyclops game object
            NitroxId id = NitroxEntity.GetId(gameObject);
            Optional<CyclopsModel> model = NitroxServiceLocator.LocateService<Vehicles>().TryGetVehicle<CyclopsModel>(id);
            if (!model.HasValue)
            {
                Log.Error($"{nameof(CyclopsLightingPanel_OnSubConstructionComplete_Patch)}: Could not find {nameof(CyclopsModel)} by Nitrox id {id}.\nGO containing wrong id: {__instance.GetFullHierarchyPath()}");
                return false;
            }

            return model.Value.FloodLightsOn;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
