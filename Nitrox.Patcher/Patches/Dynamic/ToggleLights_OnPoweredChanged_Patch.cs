using System;
using System.Reflection;
using Harmony;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using UnityEngine;

namespace Nitrox.Patcher.Patches.Dynamic
{
    class ToggleLights_OnPoweredChanged_Patch : NitroxPatch, IDynamicPatch
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
                NitroxId id = NitroxEntity.GetId(gameObject);
                SeamothModel model = NitroxServiceLocator.LocateService<Vehicles>().GetVehicles<SeamothModel>(id);           
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
