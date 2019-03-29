using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    class CyclopsLightingPanel_OnSubConstructionComplete_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsLightingPanel);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SubConstructionComplete", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(CyclopsLightingPanel __instance)
        {
            // Suppress powered on if a cyclops´s floodlight is set to false            
            GameObject gameObject = __instance.gameObject;
            string guid = GuidHelper.GetGuid(gameObject);
            CyclopsModel model = NitroxServiceLocator.LocateService<Vehicles>().GetVehicles<CyclopsModel>(guid);
            
            return model.FloodLightsOn;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
