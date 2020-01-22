using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    class CyclopsLightingPanel_OnSubConstructionComplete_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsLightingPanel);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SubConstructionComplete", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(CyclopsLightingPanel __instance)
        {
            // Suppress powered on if a cyclops´s floodlight is set to false            
            GameObject gameObject = __instance.gameObject;
            NitroxId id = NitroxIdentifier.GetId(gameObject);
            CyclopsModel model = NitroxServiceLocator.LocateService<Vehicles>().GetVehicles<CyclopsModel>(id);
            
            return model.FloodLightsOn;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
