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
    class Seamoth_SubConstructionComplete_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SeaMoth t) => t.SubConstructionComplete());

        public static bool Prefix(SeaMoth __instance)
        {
            // Suppress powered on if a seamoth´s default is set to false            
            GameObject gameObject = __instance.gameObject;
            NitroxId id = NitroxEntity.GetId(gameObject);
            Optional<SeamothModel> model = NitroxServiceLocator.LocateService<Vehicles>().TryGetVehicle<SeamothModel>(id);

            if (!model.HasValue)
            {
                Log.Error($"{nameof(Seamoth_SubConstructionComplete_Patch)}: Could not find {nameof(CyclopsModel)} by Nitrox id {id}.\nGO containing wrong id: {__instance.GetFullHierarchyPath()}");
                return false;
            }

            // Set lights of seamoth            
            Validate.NotNull(__instance.toggleLights, $"toggleLights is Null on {__instance.gameObject.name} {__instance.transform.position}");
            __instance.toggleLights.SetLightsActive(model.Value.LightOn);
            return model.Value.LightOn;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
