using System;
using System.Reflection;
using HarmonyLib;
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
    class Seamoth_SubConstructionComplete_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(SeaMoth);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SubConstructionComplete", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(SeaMoth __instance)
        {
            // Suppress powered on if a seamoth´s default is set to false            
            GameObject gameObject = __instance.gameObject;
            NitroxId id = NitroxEntity.GetId(gameObject);
            Optional<SeamothModel> model = NitroxServiceLocator.LocateService<Vehicles>().TryGetVehicle<SeamothModel>(id);
            
            if (!model.HasValue)
            {
                Log.Error($"{nameof(Seamoth_SubConstructionComplete_Patch)}: Could not find {nameof(CyclopsModel)} by Nitrox id {id}.\nGO containing wrong id: {__instance.GetHierarchyPath()}");
                return false;
            }

            // Set lights of seamoth            
            ToggleLights toggleLights = gameObject.RequireComponentInChildren<ToggleLights>();
            toggleLights.lightsActive = model.Value.LightOn;
            return model.Value.LightOn;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
