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
        public static readonly Type TARGET_CLASS = typeof(Rocket);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("CallElevator", BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(Rocket __instance, bool up)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);
            NitroxServiceLocator.LocateService<Rockets>().BroadcastElevatorCall(id, up);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}


