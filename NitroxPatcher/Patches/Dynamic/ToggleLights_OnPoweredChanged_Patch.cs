using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    class ToggleLights_OnPoweredChanged_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((ToggleLights t) => t.OnPoweredChanged(default(bool)));

        public static bool Prefix(ToggleLights __instance, bool powered)
        {
            // Find the right gameobject in the hierarchy to sync on:
            // Suppress powered on if a seamoth´s default is set to false            
            if (__instance.GetComponentInParent<SeaMoth>() != null && powered)
            {
                GameObject gameObject = __instance.transform.parent.gameObject;
                NitroxId id = NitroxEntity.GetId(gameObject);
                SeamothModel model = NitroxServiceLocator.LocateService<Vehicles>().GetVehicles<SeamothModel>(id);
                return model.LightOn == __instance.lightsActive;
            }

            return true;
        }


        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
