using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Seamoth_SubConstructionComplete_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(SeaMoth).GetMethod("SubConstructionComplete", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(SeaMoth __instance)
        {
            // Suppress powered on if a seamoth´s default is set to false            
            GameObject gameObject = __instance.gameObject;
            NitroxId id = NitroxEntity.GetId(gameObject);
            SeamothModel model = NitroxServiceLocator.LocateService<Vehicles>().GetVehicles<SeamothModel>(id);
            
            // Set lights of seamoth            
            ToggleLights toggleLights = gameObject.RequireComponentInChildren<ToggleLights>();
            toggleLights.lightsActive = model.LightOn;
            return model.LightOn;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
