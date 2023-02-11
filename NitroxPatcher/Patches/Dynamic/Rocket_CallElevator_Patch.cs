using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Rocket_CallElevator_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Rocket t) => t.CallElevator(default(bool)));

        public static void Prefix(Rocket __instance, out Rocket.RocketElevatorStates __state)
        {
            __state = __instance.elevatorState;
        }

        public static void Postfix(Rocket __instance, bool up, Rocket.RocketElevatorStates __state)
        {
            if (__state != __instance.elevatorState)
            {
                GameObject gameObject = __instance.gameObject;
                NitroxId id = NitroxEntity.GetId(gameObject);

                Resolve<Entities>().EntityMetadataChanged(__instance, id);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, prefix:true, postfix:true);
        }
    }
}
