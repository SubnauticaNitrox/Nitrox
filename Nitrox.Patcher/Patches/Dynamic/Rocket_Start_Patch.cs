using System.Reflection;
using Harmony;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Client.Unity.Helper;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using Nitrox.Model.Logger;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using UnityEngine;

namespace Nitrox.Patcher.Patches.Dynamic
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

            if (__instance.currentRocketStage > 0)
            {
                __instance.elevatorState = model.Value.ElevatorUp ? Rocket.RocketElevatorStates.AtTop : Rocket.RocketElevatorStates.AtBottom;
                __instance.elevatorPosition = model.Value.ElevatorUp ? 1f : 0f;
                __instance.ReflectionCall("SetElevatorPosition", false, false);

                //CockpitSwitch and RocketPreflightCheckScreenElement are filled based on the RocketPreflightCheckManager
                if (__instance.currentRocketStage > 3)
                {
                    RocketPreflightCheckManager rocketPreflightCheckManager = gameObject.RequireComponent<RocketPreflightCheckManager>();
                    rocketPreflightCheckManager.preflightChecks.AddRange(model.Value.PreflightChecks);
                }
            }

            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
