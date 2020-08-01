using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Openable_PlayOpenAnimation_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Openable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("PlayOpenAnimation", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(Openable __instance, bool openState, float duration)
        {
            if (__instance.isOpen != openState)
            {
                NitroxId id = NitroxEntity.GetId(__instance.gameObject);
                NitroxServiceLocator.LocateService<Interior>().OpenableStateChanged(id, openState, duration);
            }

            return true;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
