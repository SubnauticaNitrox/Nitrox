using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Rocket_CallElevator_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(Rocket).GetMethod("CallElevator", BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(Rocket __instance, Rocket.RocketElevatorStates __state)
        {
            __state = __instance.elevatorState;
        }

        public static void Postfix(Rocket __instance, bool up, Rocket.RocketElevatorStates __state)
        {
            if (__instance.elevatorState != __state)
            {
                NitroxId id = NitroxEntity.GetId(__instance.gameObject);
                NitroxServiceLocator.LocateService<Rockets>().BroadcastElevatorCall(id, up);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}


