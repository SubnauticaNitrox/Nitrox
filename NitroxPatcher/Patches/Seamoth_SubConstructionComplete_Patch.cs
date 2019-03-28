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
using NitroxModel.Logger;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    class Seamoth_SubConstructionComplete_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(SeaMoth);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SubConstructionComplete", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(SeaMoth __instance)
        {
            // Suppress powered on if a seamoth´s default is set to false            
            GameObject gameObject = __instance.gameObject;
            string guid = GuidHelper.GetGuid(gameObject);
            SeamothModel model = NitroxServiceLocator.LocateService<Vehicles>().GetVehicles<SeamothModel>(guid);


            // Set lights of seamoth            
            Log.Debug("Set lights of seamoth");
            ToggleLights toggleLights = gameObject.GetComponent<ToggleLights>();
            if (toggleLights == null)
            {
                Log.Debug("ToggleLight is in children");
                toggleLights = gameObject.RequireComponentInChildren<ToggleLights>();
            }
            toggleLights.lightsActive = model.LightOn;
            return model.LightOn;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
