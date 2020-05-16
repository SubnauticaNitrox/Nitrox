using System;
using System.Reflection;
using Harmony;
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
    public class Rocket_Start_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Rocket);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(Rocket __instance)
        {
            GameObject gameObject = __instance.gameObject;
            NitroxId id = NitroxEntity.GetId(gameObject);
            Optional<NeptuneRocketModel> model = NitroxServiceLocator.LocateService<Vehicles>().TryGetVehicle<NeptuneRocketModel>(id);

            if (!model.HasValue)
            {
                Log.Error($"{nameof(Rocket_Start_Patch)}: Could not find {nameof(NeptuneRocketModel)} by Nitrox id {id}.\nGO containing wrong id: {__instance.GetHierarchyPath()}");
                return false;
            }

            __instance.currentRocketStage = model.Value.CurrentRocketStage;
            //__instance.elevatorState = model.Value.ElevatorUp ? Rocket.RocketElevatorStates.AtTop : Rocket.RocketElevatorStates.AtBottom;

            Optional<ElevatorCallControl> opelevator = Optional.OfNullable(gameObject.GetComponentInChildren<ElevatorCallControl>());

            if (opelevator.HasValue)
            {
                opelevator.Value.elevatorUp = model.Value.ElevatorUp;
            }
            else
            {
                Log.Error($"{nameof(Rocket_Start_Patch)}: Unable to find the ElevatorCallControl inside the RocketBase");
            }

            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
