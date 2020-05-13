using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    class Rocket_SubConstructionComplete_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Rocket);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SubConstructionComplete", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(Rocket __instance)
        {        
            GameObject gameObject = __instance.gameObject;
            NitroxId id = NitroxEntity.GetId(gameObject);
            Optional<NeptuneRocketModel> model = NitroxServiceLocator.LocateService<Vehicles>().TryGetVehicle<NeptuneRocketModel>(id);

            if (!model.HasValue)
            {
                Log.Error($"{nameof(Seamoth_SubConstructionComplete_Patch)}: Could not find {nameof(NeptuneRocketModel)} by Nitrox id {id}.\nGO containing wrong id: {__instance.GetHierarchyPath()}");
                return false;
            }

            __instance.currentRocketStage = model.Value.CurrentRocketStage;
            __instance.ReflectionCall("Start", false, false);

            __instance.rocketConstructor?.SetActive(true);


            Optional<RocketConstructorInput> optional = Optional.OfNullable(gameObject.GetComponentInChildren<RocketConstructorInput>());
            if (optional.HasValue)
            {
                optional.Value.gameObject.SetActive(true);
            }
            else
            {
                Log.Error("[ERROR] Unable to find the RocketConstructorInput inside the RocketBase");
            } 

            ElevatorCallControl elevatorCallControl = gameObject.RequireComponentInChildren<ElevatorCallControl>();
            elevatorCallControl.elevatorUp = model.Value.ElevatorUp;
            elevatorCallControl.gameObject.SetActive(true);

            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
