using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using System.Reflection;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Rocket_Start_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(Rocket).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(Rocket __instance)
        {
            GameObject gameObject = __instance.gameObject;
            NitroxId id = NitroxEntity.GetId(gameObject);
            Optional<NeptuneRocketModel> model = NitroxServiceLocator.LocateService<Vehicles>().TryGetVehicle<NeptuneRocketModel>(id);

            if (!model.HasValue)
            {
                Log.Error($"{nameof(Rocket_Start_Patch)}: Could not find NeptuneRocketModel by Nitrox id {id}.\nGO containing wrong id: {__instance.GetHierarchyPath()}");
                return false;
            }

            __instance.currentRocketStage = model.Value.CurrentStage;

            RocketConstructor rocketConstructor = gameObject.GetComponentInChildren<RocketConstructor>(true);
            if (rocketConstructor)
            {
                NitroxEntity.SetNewId(rocketConstructor.gameObject, model.Value.ConstructorId);
            }
            else
            {
                Log.Error($"{nameof(Rocket_Start_Patch)}: Could not find attached RocketConstructor to rocket with id {id}");
            }

            if (__instance.currentRocketStage > 0)
            {
                __instance.elevatorState = model.Value.ElevatorUp ? Rocket.RocketElevatorStates.AtTop : Rocket.RocketElevatorStates.AtBottom;
                __instance.elevatorPosition = model.Value.ElevatorUp ? 1f : 0f;
                __instance.ReflectionCall("SetElevatorPosition", false, false);
            }

            return true;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
