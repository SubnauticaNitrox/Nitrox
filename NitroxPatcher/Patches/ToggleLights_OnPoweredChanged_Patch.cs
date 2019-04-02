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
    class ToggleLights_OnPoweredChanged_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ToggleLights);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnPoweredChanged", BindingFlags.NonPublic | BindingFlags.Instance);


        public static bool Prefix(ToggleLights __instance, bool powered)
        {
            // Find the right gameobject in the hierarchy to sync on:
            GameObject gameObject = null;
            // Suppress powered on if a seamoth´s default is set to false            
            if (__instance.GetComponentInParent<SeaMoth>() != null && powered)
            {                
                gameObject = __instance.transform.parent.gameObject;
                string guid = GuidHelper.GetGuid(gameObject);
                SeamothModel model = NitroxServiceLocator.LocateService<Vehicles>().GetVehicles<SeamothModel>(guid);           
                return (model.LightOn == __instance.lightsActive);
            }        
            
            return true;            
        }


        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
