using System;
using System.Reflection;
using Harmony;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Client.Unity.Helper;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Logger;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using UnityEngine;

namespace Nitrox.Patcher.Patches.Dynamic
{
    class CyclopsLightingPanel_OnSubConstructionComplete_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsLightingPanel);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SubConstructionComplete", BindingFlags.Public | BindingFlags.Instance);

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
