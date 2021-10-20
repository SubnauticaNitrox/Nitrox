using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel_Subnautica.Packets;
using UnityEngine;
using static Rocket;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Rocket_ElevatorControlButtonActivate_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Rocket t) => t.ElevatorControlButtonActivate());

        public static void Prefix(Rocket __instance, out RocketElevatorStates __state)
        {
            __state = __instance.elevatorState;
        }

        public static void Postfix(Rocket __instance, RocketElevatorStates __state)
        {
            if (__state != __instance.elevatorState)
            {
                Rockets rocket = NitroxServiceLocator.LocateService<Rockets>();
                GameObject gameObject = __instance.gameObject;
                NitroxId id = NitroxEntity.GetId(gameObject);

                bool isGoingUp = __instance.elevatorState == RocketElevatorStates.Up || __instance.elevatorState == RocketElevatorStates.AtTop;
                rocket.CallElevator(id, RocketElevatorPanel.INTERNAL_PANEL, isGoingUp);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, prefix:true, postfix:true);
        }
    }
}
