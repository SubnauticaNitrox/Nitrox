using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Persistent
{
    class EscapePodFirstUseCinematicsController_Initialize_Patch : NitroxPatch, IPersistentPatch
    {
        public static readonly Type TARGET_CLASS = typeof(EscapePodFirstUseCinematicsController);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(EscapePodFirstUseCinematicsController __instance)
        {
            __instance.bottomCinematicTarget.gameObject.SetActive(true);
            __instance.topCinematicTarget.gameObject.SetActive(true);

            __instance.bottomFirstUseCinematicTarget.gameObject.SetActive(false);
            __instance.topFirstUseCinematicTarget.gameObject.SetActive(false);
            
            return !Multiplayer.Active;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
