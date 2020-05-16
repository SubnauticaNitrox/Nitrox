using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Rocket_ElevatorControlButtonActivate_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Rocket);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("ElevatorControlButtonActivate", BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(Rocket __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);
            bool up = __instance.elevatorState == Rocket.RocketElevatorStates.AtBottom;
            NitroxServiceLocator.LocateService<Rockets>().BroadcastElevatorCall(id, up);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}

