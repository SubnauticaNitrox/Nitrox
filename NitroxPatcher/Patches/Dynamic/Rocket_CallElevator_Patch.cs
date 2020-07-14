using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Rocket_CallElevator_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(Rocket).GetMethod("CallElevator", BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(Rocket __instance, out Rocket.RocketElevatorStates __state)
        {
            __state = __instance.elevatorState;
        }

        public static void Postfix(Rocket __instance, bool up, Rocket.RocketElevatorStates __state)
        {
            if (__state != __instance.elevatorState)
            {
                Rockets rocket = NitroxServiceLocator.LocateService<Rockets>();
                GameObject gameObject = __instance.gameObject;
                NitroxId id = NitroxEntity.GetId(gameObject);

                rocket.CallElevator(id, up);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
