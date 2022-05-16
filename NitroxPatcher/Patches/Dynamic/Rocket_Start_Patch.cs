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
    public class Rocket_Start_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Rocket t) => t.Start());

        public static bool Prefix(Rocket __instance)
        {
            GameObject gameObject = __instance.gameObject;
            NitroxId id = NitroxEntity.GetId(gameObject);

            Optional<NeptuneRocketModel> model = NitroxServiceLocator.LocateService<Vehicles>().TryGetVehicle<NeptuneRocketModel>(id);
            if (!model.HasValue)
            {
                Log.Error($"{nameof(Rocket_Start_Patch)}: Could not find NeptuneRocketModel by Nitrox id {id}.\nGO containing wrong id: {__instance.GetFullHierarchyPath()}");
                return false;
            }

            __instance.currentRocketStage = model.Value.CurrentStage;

            if (__instance.currentRocketStage > 0)
            {
                __instance.elevatorState = model.Value.ElevatorUp ? Rocket.RocketElevatorStates.AtTop : Rocket.RocketElevatorStates.AtBottom;
                __instance.elevatorPosition = model.Value.ElevatorUp ? 1f : 0f;
                __instance.SetElevatorPosition();

                //CockpitSwitch and RocketPreflightCheckScreenElement are filled based on the RocketPreflightCheckManager
                if (__instance.currentRocketStage > 3)
                {
                    RocketPreflightCheckManager rocketPreflightCheckManager = gameObject.RequireComponent<RocketPreflightCheckManager>();
                    rocketPreflightCheckManager.preflightChecks.AddRange(model.Value.PreflightChecks);
                }
            }

            return true;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
